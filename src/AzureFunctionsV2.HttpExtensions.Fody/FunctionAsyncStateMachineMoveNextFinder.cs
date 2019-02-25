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

                // If the request is never used in the Function body, it will be optimized out in Release builds.
                // Therefore if it's missing, we must add it as a field manually because our functionality depends on it.
                var httpRequestFieldInStateMachineType = matchingStateMachineType.Fields.FirstOrDefault(f => f.FieldType.Name == "HttpRequest");
                if (httpRequestFieldInStateMachineType == null)
                {
                    log("  - No HttpRequest field present in the state machine even though it is in the signature; most likely optimized out. Inserting it in.");
                    // continue;
                    var httpRequestParam = functionMethod.Parameters.First(p => p.ParameterType.Name == "HttpRequest");
                    var httpRequestFieldDef = new FieldDefinition(httpRequestParam.Name, FieldAttributes.Public, httpRequestParam.ParameterType);
                    httpRequestFieldInStateMachineType = httpRequestFieldDef;
                    matchingStateMachineType.Fields.Add(httpRequestFieldDef);

                    var ctorParamIndex = functionMethod.Parameters.IndexOf(httpRequestParam);
                    log("  - Index of ctor HttpRequest param is " + ctorParamIndex);
                    // After instantiating the state machine (newobj instr with the operand of type matchingStateMachineType)
                    // expected: newobj, stloc, ldloc
                    // ldarg at parameter index
                    // stfld for the newly created field

                    // WHEN OPTIMIZED:
                    //IL_0000: ldloca.s V_0
                    //IL_0002: call valuetype[System.Threading.Tasks]System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1 < !0/*class [Microsoft.AspNetCore.Mvc.Abstractions]Microsoft.AspNetCore.Mvc.IActionResult*/> valuetype[System.Threading.Tasks]System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1 <class [Microsoft.AspNetCore.Mvc.Abstractions] Microsoft.AspNetCore.Mvc.IActionResult>::Create()
                    //IL_0007: stfld valuetype[System.Threading.Tasks]System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1<class [Microsoft.AspNetCore.Mvc.Abstractions] Microsoft.AspNetCore.Mvc.IActionResult> AzureFunctionsV2.HttpExtensions.Examples.Authorization.AuthorizedFuncs/'<BasicAuthenticatedFunc>d__0'::'<>t__builder'
                    //IL_000c: ldloca.s V_0

                    // WHEN NOT OPTIMIZED:
                    //IL_0000: newobj instance void AzureFunctionsV2.HttpExtensions.Tests.FunctionApp.AuthTests / '<AuthTest1>d__0'::.ctor()
                    //IL_0005: stloc.0      // V_0
                    //IL_0006: ldloc.0      // V_0

                    var localStateMachineVar = functionMethod.Body.Variables.First(v => v.VariableType == matchingStateMachineType);
                    var localStateMachineVarIndex = functionMethod.Body.Variables.IndexOf(localStateMachineVar);
                    var instructionIndex = 0;
                    var newInstructions = new[]
                    {
                        // ldloc load the local that holds the state machine instance.
                        functionMethod.Body.GetILProcessor().Create(OpCodes.Ldloca, localStateMachineVarIndex),
                        functionMethod.Body.GetILProcessor().Create(OpCodes.Ldarg, ctorParamIndex),
                        //Instruction.Create(OpCodes.Ldarg_0),
                        Instruction.Create(OpCodes.Stfld, httpRequestFieldDef)
                    };
                    foreach (var instruction in newInstructions)
                    {
                        functionMethod.Body.Instructions.Insert(instructionIndex, instruction);
                        instructionIndex++;
                    }

                }

                functionMethodDefinitions.Add((moveNextMethod, httpRequestFieldInStateMachineType, functionMethod.FullName));

            }

            log("Discovery completed.");

            return functionMethodDefinitions;
        }

    }
}
