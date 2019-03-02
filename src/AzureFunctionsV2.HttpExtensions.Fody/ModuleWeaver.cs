using System;
using System.Collections.Generic;
using System.Linq;
using AzureFunctionsV2.HttpExtensions.ILInjects;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace AzureFunctionsV2.HttpExtensions.Fody
{
    /// <summary>
    /// The Fody Weaver that inserts the extra exception swallowing IL assembly code inside each HTTP Function.
    /// </summary>
    public class ModuleWeaver : BaseModuleWeaver
    {
        /// <summary>
        /// Find or manufacture a SetResult() instruction.
        /// If we're dealing with a normal try-catch block, we'll find the SetResult easily and can just
        /// refer to the existing instruction. If not, we need to instantiate a new instruction.
        /// This is a more complex affair; we try to find SetException() (this happens when optimization
        /// removes SetResult() completely, ie. in cases where a method always throws) instead, to
        /// get a reference to the AsyncTaskMethodBuilder class and work from there to monkey up
        /// an generic instance of the SetResult() method we want to call.
        /// Tad bit ugly, but it works.
        /// </summary>
        /// <param name="instructions"></param>
        /// <param name="stateMachineType"></param>
        /// <returns></returns>
        private Instruction FindOrCreateSetResultInstruction(IEnumerable<Instruction> instructions, TypeDefinition stateMachineType)
        {
            var existingInstruction = instructions.LastOrDefault(inst => inst.Operand is MethodReference mr &&
                                              mr.Name == "SetResult" &&
                                              mr.DeclaringType.FullName.StartsWith(
                                                  "System.Runtime.CompilerServices.AsyncTaskMethodBuilder"));

            if (existingInstruction != null)
                return existingInstruction;

            var setException = instructions.LastOrDefault(inst => inst.Operand is MethodReference mr &&
                                               mr.Name == "SetException" &&
                                               mr.DeclaringType.FullName.StartsWith(
                                                   "System.Runtime.CompilerServices.AsyncTaskMethodBuilder"));
            if (setException != null && setException.Operand is MethodReference setExceptionOperand)
            {
                try
                {
                    var tBuilderType = stateMachineType.Fields.FirstOrDefault(f => f.Name.EndsWith("t__builder"))
                        .FieldType;

                    var asyncTaskMethodBuilderType = setExceptionOperand.DeclaringType.Resolve();
                    var iActionResultType =
                        ((GenericInstanceType) tBuilderType).GenericArguments.FirstOrDefault().Resolve();

                    var genericAsyncTaskMethodBuilderType =
                        asyncTaskMethodBuilderType.MakeGenericInstanceType(iActionResultType);

                    var setResult = Helpers.MakeHostInstanceGeneric(
                        genericAsyncTaskMethodBuilderType.Resolve().Methods.First(m => m.Name == "SetResult"),
                        iActionResultType);

                    if (setResult != null)
                    {
                        var setResultMethodReference = ModuleDefinition.ImportReference(setResult);
                        return Instruction.Create(OpCodes.Call, setResultMethodReference);
                    }
                }
                catch (Exception e)
                {
                    return null;
                }

            }

            return null;
        }

        private ExceptionHandler FindFirstExceptionHandler(MethodBody methodBody)
        {
            return methodBody.ExceptionHandlers.FirstOrDefault();
        }

        private int FindSetExceptionInstructionIndex(IList<Instruction> instructions)
        {
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].Operand is MethodReference methodReference)
                {
                    if (methodReference.Name == "SetException" &&
                        methodReference.DeclaringType.FullName.StartsWith(
                            "System.Runtime.CompilerServices.AsyncTaskMethodBuilder"))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public override void Execute()
        {
            LogInfo(
                "Will attempt to find any Azure Functions (static async methods in static classes with 'FunctionNameAttribute'), " +
                "and then augment them with IL instructions that call exception handling code if an exception gets thrown.");

            var httpExtensionsAssemblyName = typeof(ILFunctionExceptionHandler).Assembly.FullName;
            var referencedAsm = ModuleDefinition.AssemblyResolver.Resolve(AssemblyNameReference.Parse(httpExtensionsAssemblyName));

            var rethrowerFunctionDefinition = referencedAsm.MainModule.GetType(typeof(ILFunctionExceptionHandler).FullName).GetMethods()
                .First(m => m.Name == "RethrowStoredException");

            var rethrowerFunctionReference = ModuleDefinition.ImportReference(rethrowerFunctionDefinition);


            var functionExceptionHandlerDefinition = referencedAsm.MainModule
                .GetType(typeof(ILFunctionExceptionHandler).FullName).GetMethods()
                .First(m => m.Name == "HandleExceptionAndReturnResult");
            var functionExceptionHandlerReference =
                ModuleDefinition.ImportReference(functionExceptionHandlerDefinition);

            var functionMethods = FunctionAsyncStateMachineMoveNextFinder.Find(ModuleDefinition, LogInfo);

            foreach (var funcMethod in functionMethods)
            {
                var instructions = funcMethod.CompilerGeneratedMoveNext.Body.Instructions;

                if (FindOrCreateSetResultInstruction(instructions, funcMethod.CompilerGeneratedStateMachineType) == null)
                {
                    LogWarning("No SetResult instruction found in method body and failed to create a reference to it, will not apply exception handling " +
                               $"for method '{funcMethod.SourceFunctionName}' " +
                               "- this method will return empty 500 results upon exceptions");
                    continue;
                }

                if (FindSetExceptionInstructionIndex(instructions) == -1)
                {
                    LogWarning(
                        "Couldn't find the SetException instruction in the method, unable to apply exception handling " +
                        $"for method '{funcMethod.SourceFunctionName}'.");
                    continue;
                }

                // Unoptimize first, to not break things.
                funcMethod.CompilerGeneratedMoveNext.Body.SimplifyMacros();

                var tryCatchBlock = funcMethod.CompilerGeneratedMoveNext.Body.ExceptionHandlers.First();
                var handlerCallInstructionIndex = 0; // insert to the very beginning.
                int setExceptionInstructionIndex = FindSetExceptionInstructionIndex(instructions);
                var setResultInstruction = FindOrCreateSetResultInstruction(instructions, funcMethod.CompilerGeneratedStateMachineType);

                LogInfo($"Augmenting function '{funcMethod.SourceFunctionName}' with exception handling logic.");

                // Insert the call to rethrower to the beginning, which will basically rethrow any
                // exception that the function filters may have handled with a try-catch and stored.                

                var newInstructions = new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldfld, funcMethod.HttpRequestFieldDefinition),
                    Instruction.Create(OpCodes.Call, rethrowerFunctionReference)
                };
                foreach (var newInstruction in newInstructions)
                {
                    instructions.Insert(handlerCallInstructionIndex, newInstruction);
                    handlerCallInstructionIndex++;
                    setExceptionInstructionIndex++;
                    tryCatchBlock.TryStart = instructions.First();
                }

                // Remove the default generated SetException() call, as we don't want the async method to fail.
                instructions.RemoveAt(setExceptionInstructionIndex);

                // Insert new instructions: call our custom exception handler, which will return an IActionResult value.
                // Call the SetResult() with the IActionResult value in the stack.
                // Insert a nop just to mark the block.
                newInstructions = new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0), 
                    Instruction.Create(OpCodes.Ldfld, funcMethod.HttpRequestFieldDefinition), 
                    Instruction.Create(OpCodes.Call, functionExceptionHandlerReference),
                    setResultInstruction,
                    Instruction.Create(OpCodes.Nop)
                };
                foreach (var newInstruction in newInstructions)
                {
                    instructions.Insert(setExceptionInstructionIndex, newInstruction);
                    setExceptionInstructionIndex++;
                }

                // Re-optimize again.
                funcMethod.CompilerGeneratedMoveNext.Body.OptimizeMacros();

            }

            LogInfo("All methods processed.");
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            return Enumerable.Empty<string>();
        }
    }
}
