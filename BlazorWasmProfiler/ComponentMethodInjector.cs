using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace BlazorWasmProfiler
{
    public class ComponentMethodInjector : Task
    {
        [Required]
        public string AssemblyPath { get; set; } = string.Empty;

        public override bool Execute()
        {
            try
            {
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(AssemblyPath);

                // Load ExecutionStatistics type and methods
                TypeReference executionStatisticsType = assembly.MainModule.ImportReference(typeof(ExecutionStatistics));
                MethodReference renderTimerStartMethod = assembly.MainModule.ImportReference(executionStatisticsType.Resolve().Methods.First(m => m.Name == "RenderTimerStart"));
                MethodReference renderTimerStopMethod = assembly.MainModule.ImportReference(executionStatisticsType.Resolve().Methods.First(m => m.Name == "RenderTimerStop"));

                foreach (ModuleDefinition module in assembly.Modules)
                {
                    foreach (TypeDefinition type in module.Types)
                    {
                        if (type.BaseType?.FullName == "Microsoft.AspNetCore.Components.ComponentBase")
                        {
                            foreach (MethodDefinition method in type.Methods)
                            {
                                if ((method.Name == "OnParametersSet" || method.Name == "OnAfterRender") &&
                                    method.IsFamily &&
                                    method.IsVirtual &&
                                    method.ReturnType.FullName == "System.Void")
                                {
                                    ILProcessor ilProcessor = method.Body.GetILProcessor();

                                    if (method.Name == "OnAfterRender")
                                    {
                                        Instruction firstInstruction = method.Body.Instructions[0];
                                        ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, type.FullName));
                                        ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, renderTimerStopMethod));
                                    }

                                    if (method.Name == "OnParametersSet")
                                    {
                                        Instruction lastInstruction = method.Body.Instructions[method.Body.Instructions.Count - 1];
                                        ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldstr, type.FullName));
                                        ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Call, renderTimerStartMethod));
                                    }
                                }
                            }
                        }
                    }
                }

                // Save the modified assembly back to the original file
                assembly.Write(AssemblyPath);
            }
            catch (Exception ex)
            {
                Log.LogError("Error while inspecting the project: " + ex.Message);
                return false;
            }

            return true;
        }

        public bool CountClasses()
        {
            try
            {
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(AssemblyPath);

                int derivedFromClassBaseCount = 0;
                int onParametersSetCount = 0;

                foreach (ModuleDefinition module in assembly.Modules)
                {
                    foreach (TypeDefinition type in module.Types)
                    {
                        if (type.BaseType?.FullName == "Microsoft.AspNetCore.Components.ComponentBase")
                        {
                            derivedFromClassBaseCount++;

                            foreach (MethodDefinition method in type.Methods)
                            {
                                if (method.Name == "OnParametersSet" &&
                                    method.IsFamily && method.IsVirtual &&
                                    method.ReturnType.FullName == "System.Void" &&
                                    method.Parameters.Count == 0)
                                {
                                    // Method "protected override void OnParametersSet()" found
                                    onParametersSetCount++;
                                }
                            }
                        }
                    }
                }

                Log.LogMessage(MessageImportance.High, $"Number of classes derived from Microsoft.AspNetCore.Components.ComponentBase: {derivedFromClassBaseCount}");
                Log.LogMessage(MessageImportance.High, $"Number of classes with \"protected override void OnParametersSet()\": {onParametersSetCount}");
            }
            catch (Exception ex)
            {
                Log.LogError("Error while inspecting the project: " + ex.Message);

                return false;
            }

            return true;
        }
    }
}