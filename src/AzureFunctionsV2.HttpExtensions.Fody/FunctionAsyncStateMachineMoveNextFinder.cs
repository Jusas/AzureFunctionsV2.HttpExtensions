using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace AzureFunctionsV2.HttpExtensions.Fody
{
    public static class FunctionAsyncStateMachineMoveNextFinder
    {
        public static List<(MethodDefinition compilerGenerated, FieldDefinition httpRequestFieldDefinition, string sourceFunctionName)> Find(ModuleDefinition moduleDefinition,
           Action<string> log)
        {
            // Find static class functions, that have a specific attribute.
            // Try to get its System.Runtime.CompilerServices.AsyncStateMachineAttribute, and get its Type.
            // Get its MoveNext(), and then proceed as with AsyncErrorHandler.
            List<(MethodDefinition, FieldDefinition, string)> functionMethodDefinitions =
                new List<(MethodDefinition compilerGenerated, FieldDefinition httpRequestFieldDefinition, string sourceFunctionName)>();

            List<MethodDefinition> temp = new List<MethodDefinition>();
            foreach (var typeDefinition in moduleDefinition.Types)
            {
                if (typeDefinition.IsAbstract && typeDefinition.IsClass && typeDefinition.IsSealed)
                {
                    var functionLikeMethods = typeDefinition.Methods.Where(m =>
                        m.HasCustomAttributes &&
                        m.CustomAttributes.Any(a => a.AttributeType.Name == "FunctionNameAttribute") &&
                        m.CustomAttributes.Any(a => a.AttributeType.Name == "AsyncStateMachineAttribute") &&
                        m.Parameters.Any(p => p.ParameterType.Name == "HttpRequest"));
                    if (functionLikeMethods.Any())
                        temp.AddRange(functionLikeMethods);
                }
            }

            foreach (var functionMethod in temp)
            {

                log("Found Function " + functionMethod.FullName);
                var compilerGeneratedStateMachineType = functionMethod.Body.Variables.FirstOrDefault(v =>
                        v.VariableType.Resolve().Interfaces.Any(i => i.InterfaceType.Name == "IAsyncStateMachine"))
                    ?.VariableType;

                log("  - Corresponding compiler generated state machine: " + compilerGeneratedStateMachineType.FullName);
                var allModuleTypes =
                    moduleDefinition.Types.Concat(moduleDefinition.Types.SelectMany(t => t.NestedTypes));

                var matchingStateMachineType = allModuleTypes.First(
                    t => t.FullName == compilerGeneratedStateMachineType.FullName);
                var moveNextMethod = matchingStateMachineType.Methods.First(m => m.Name == "MoveNext");
                var httpRequestFieldInStateMachineType = matchingStateMachineType.Fields.First(f => f.FieldType.Name == "HttpRequest");
                functionMethodDefinitions.Add((moveNextMethod, httpRequestFieldInStateMachineType, functionMethod.FullName));

            }

            log("Discovery completed.");

            return functionMethodDefinitions;
        }

    }
}
