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
    public class ModuleWeaver : BaseModuleWeaver
    {
        private Instruction FindSetResultInstruction(IEnumerable<Instruction> instructions)
        {
            return instructions.LastOrDefault(inst => inst.Operand is MethodReference mr &&
                                              mr.Name == "SetResult" &&
                                              mr.DeclaringType.FullName.StartsWith(
                                                  "System.Runtime.CompilerServices.AsyncTaskMethodBuilder"));
        }

        private int FindFirstFunctionInstructionIndex(IList<Instruction> instructions)
        {
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].OpCode == OpCodes.Nop)
                {
                    return i + 1;
                }
            }

            return -1;
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

            var httpExtensionsAssemblyName = typeof(FunctionExceptionHandler).Assembly.FullName;
            var referencedAsm = ModuleDefinition.AssemblyResolver.Resolve(AssemblyNameReference.Parse(httpExtensionsAssemblyName));

            var rethrowerFunctionDefinition = referencedAsm.MainModule.GetType(typeof(FunctionExceptionHandler).FullName).GetMethods()
                .First(m => m.Name == "RethrowStoredException");

            var rethrowerFunctionReference = ModuleDefinition.ImportReference(rethrowerFunctionDefinition);


            var functionExceptionHandlerDefinition = referencedAsm.MainModule
                .GetType(typeof(FunctionExceptionHandler).FullName).GetMethods()
                .First(m => m.Name == "HandleExceptionAndReturnResult");
            var functionExceptionHandlerReference =
                ModuleDefinition.ImportReference(functionExceptionHandlerDefinition);

            var functionMethods = FunctionAsyncStateMachineMoveNextFinder.Find(ModuleDefinition, LogInfo);

            foreach (var funcMethod in functionMethods)
            {
                var instructions = funcMethod.compilerGenerated.Body.Instructions;

                if (FindSetResultInstruction(instructions) == null)
                {
                    LogWarning("No SetResult instruction found in method body, will not apply exception handling " +
                               $"for method '{funcMethod.sourceFunctionName}' " +
                               "- this method is probably always throwing.");
                    continue;
                }

                if (FindFirstFunctionInstructionIndex(instructions) == -1)
                {
                    LogWarning(
                        "Couldn't find the beginning of the method (first nop OpCode), unable to apply exception handling " +
                        $"for method '{funcMethod.sourceFunctionName}'.");
                    continue;
                }

                if (FindSetExceptionInstructionIndex(instructions) == -1)
                {
                    LogWarning(
                        "Couldn't find the SetException instruction in the method, unable to apply exception handling " +
                        $"for method '{funcMethod.sourceFunctionName}'.");
                    continue;
                }

                // Unoptimize first, to not break things.
                funcMethod.compilerGenerated.Body.SimplifyMacros();

                int handlerCallInstructionIndex = FindFirstFunctionInstructionIndex(instructions);
                int setExceptionInstructionIndex = FindSetExceptionInstructionIndex(instructions);
                var setResultInstruction = FindSetResultInstruction(instructions);

                LogInfo($"Augmenting function '{funcMethod.sourceFunctionName}' with exception handling logic.");

                // Insert the call to rethrower to the beginning, which will basically rethrow any
                // exception that the function filters may have handled with a try-catch and stored.                

                var newInstructions = new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldfld, funcMethod.httpRequestFieldDefinition),
                    Instruction.Create(OpCodes.Call, rethrowerFunctionReference)
                };
                foreach (var newInstruction in newInstructions)
                {
                    instructions.Insert(handlerCallInstructionIndex, newInstruction);
                    handlerCallInstructionIndex++;
                    setExceptionInstructionIndex++;
                }

                // Remove the default generated SetException() call, as we don't want the async method to fail.
                instructions.RemoveAt(setExceptionInstructionIndex);

                // Insert new instructions: call our custom exception handler, which will return an IActionResult value.
                // Call the SetResult() with the IActionResult value in the stack.
                // Insert a nop just to mark the block.
                newInstructions = new[]
                {
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
                funcMethod.compilerGenerated.Body.OptimizeMacros();

            }

            LogInfo("All methods processed.");
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            return Enumerable.Empty<string>();
        }
    }
}
