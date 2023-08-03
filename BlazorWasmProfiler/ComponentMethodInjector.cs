using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

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
                                    // Add stopwatch start at the beginning of the method
                                    ILProcessor ilProcessor = method.Body.GetILProcessor();
                                    //ilProcessor.InsertBefore(method.Body.Instructions[0], ilProcessor.Create(OpCodes.Ldstr, type.FullName));
                                    //ilProcessor.InsertBefore(method.Body.Instructions[1], ilProcessor.Create(OpCodes.Call, typeof(ExecutionStatistics).GetMethod("StartTimingMethod")));
                                    // Add stopwatch stop at the end of the method
                                    //ilProcessor.InsertBefore(method.Body.Instructions[method.Body.Instructions.Count - 1], ilProcessor.Create(OpCodes.Ldstr, type.FullName));
                                    //ilProcessor.InsertBefore(method.Body.Instructions[method.Body.Instructions.Count - 1], ilProcessor.Create(OpCodes.Call, typeof(ExecutionStatistics).GetMethod("StopTimingMethod")));
                                }
                            }
                        }
                    }
                }
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