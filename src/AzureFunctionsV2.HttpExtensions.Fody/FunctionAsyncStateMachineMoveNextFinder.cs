using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AzureFunctionsV2.HttpExtensions.Fody
{
    public static class FunctionAsyncStateMachineMoveNextFinder
    {

        public class AsyncStateMachineContext
        {
            public MethodDefinition CompilerGeneratedMoveNext { get; set; }
            public FieldDefinition HttpRequestFieldDefinition { get; set; }
            public string SourceFunctionName { get; set; }
            public TypeDefinition CompilerGeneratedStateMachineType { get; set; }
        }

        /// <summary>
        /// Find MoveNext() methods from compiler generated async state machines that belong
        /// to an Azure Function (static methods in static classes, having FunctionNameAttribute
        /// and HttpRequest parameter in the method signature.
        /// </summary>
        /// <param name="moduleDefinition"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static List<AsyncStateMachineContext> Find(ModuleDefinition moduleDefinition,
           Action<string> log)
        {
            // Find static class functions, that have a specific attribute.
            // Try to get its System.Runtime.CompilerServices.AsyncStateMachineAttribute, and get its Type.
            // Get its MoveNext(), and then proceed as with AsyncErrorHandler.
            List<AsyncStateMachineContext> functionMethodDefinitions =
                new List<AsyncStateMachineContext>();

            List<MethodDefinition> temp = new List<MethodDefinition>();
            foreach (var typeDefinition in moduleDefinition.Types)
            {
                if (typeDefinition.IsAbstract && typeDefinition.IsClass && typeDefinition.IsSealed)
                {
                    var functionLikeMethods = typeDefinition.Methods.Where(m =>
                        m.HasCustomAttributes &&
                        m.CustomAttributes.Any(a => a.AttributeType.Name == "FunctionNameAttribute") &&
                        m.CustomAttributes.Any(a => a.AttributeType.Name == "AsyncStateMachineAttribute") &&
                        m.Parameters.Any(p => p.ParameterType.Name == "HttpRequest" && p.CustomAttributes.Any(ca => ca.AttributeType.Name == "HttpTriggerAttribute")));
                    if (functionLikeMethods.Any())
                        temp.AddRange(functionLikeMethods);
                }
            }

            foreach (var functionMethod in temp)
            {

                log("Found Function " + functionMethod.Name);
                var compilerGeneratedStateMachineType = functionMethod.Body.Variables.FirstOrDefault(v =>
                        v.VariableType.Resolve().Interfaces.Any(i => i.InterfaceType.Name == "IAsyncStateMachine"))
                    ?.VariableType;

                log("  - Corresponding compiler generated state machine: " + compilerGeneratedStateMachineType.FullName);
                var allModuleTypes =
                    moduleDefinition.Types.Concat(moduleDefinition.Types.SelectMany(t => t.NestedTypes));

                var matchingStateMachineType = allModuleTypes.First(
                    t => t.FullName == compilerGeneratedStateMachineType.FullName);
                var moveNextMethod = matchingStateMachineType.Methods.First(m => m.Name == "MoveNext");

                // If the request is never used in the Function body, it will be optimized out in Release builds.
                // Therefore if it's missing, we must add it as a field manually because our functionality depends on it.
                var httpRequestFieldInStateMachineType = matchingStateMachineType.Fields.FirstOrDefault(f => f.FieldType.Name == "HttpRequest");
                if (httpRequestFieldInStateMachineType == null)
                {
                    log("  - No HttpRequest field present in the state machine even though it is in the signature; most likely optimized out. Inserting it in.");

                    var httpRequestParam = functionMethod.Parameters.First(p => p.ParameterType.Name == "HttpRequest");
                    var httpRequestFieldDef = new FieldDefinition(httpRequestParam.Name, FieldAttributes.Public, httpRequestParam.ParameterType);
                    httpRequestFieldInStateMachineType = httpRequestFieldDef;
                    matchingStateMachineType.Fields.Add(httpRequestFieldDef);

                    var ctorParamIndex = functionMethod.Parameters.IndexOf(httpRequestParam);
                    // log("  - Index of ctor HttpRequest param is " + ctorParamIndex);
                    
                    var localStateMachineVar = functionMethod.Body.Variables.First(v => v.VariableType == matchingStateMachineType);
                    var localStateMachineVarIndex = functionMethod.Body.Variables.IndexOf(localStateMachineVar);
                    var instructionIndex = 0;
                    var newInstructions = new[]
                    {
                        // load the address of the local that holds the state machine instance.
                        functionMethod.Body.GetILProcessor().Create(OpCodes.Ldloca, localStateMachineVarIndex),
                        functionMethod.Body.GetILProcessor().Create(OpCodes.Ldarg, ctorParamIndex),
                        Instruction.Create(OpCodes.Stfld, httpRequestFieldDef)
                    };
                    foreach (var instruction in newInstructions)
                    {
                        functionMethod.Body.Instructions.Insert(instructionIndex, instruction);
                        instructionIndex++;
                    }

                }

                // functionMethodDefinitions.Add((moveNextMethod, httpRequestFieldInStateMachineType, functionMethod.Name));
                functionMethodDefinitions.Add(new AsyncStateMachineContext()
                {
                    CompilerGeneratedMoveNext = moveNextMethod,
                    SourceFunctionName = functionMethod.Name,
                    HttpRequestFieldDefinition = httpRequestFieldInStateMachineType,
                    CompilerGeneratedStateMachineType = matchingStateMachineType
                });

            }

            log("Discovery completed.");

            return functionMethodDefinitions;
        }

    }
}
