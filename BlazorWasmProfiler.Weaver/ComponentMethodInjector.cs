﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace BlazorWasmProfiler
{
    public class ComponentMethodInjector
    {
        public string AssemblyPath { get; set; } = string.Empty;

        public bool Execute()
        {
            try
            {
                //Log.LogMessage(MessageImportance.High, $"ComponentMethodInjector 1 {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}");
                //Log.LogMessage(MessageImportance.High, $"ComponentMethodInjector 1 {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");

                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(AssemblyPath);

                //Log.LogMessage(MessageImportance.High, $"ComponentMethodInjector 2 {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
                //Log.LogMessage(MessageImportance.High, $"ComponentMethodInjector 2 {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");

                // Load ExecutionStatistics type and methods
                TypeReference executionStatisticsType = assembly.MainModule.ImportReference(typeof(ExecutionStatistics));
                MethodReference renderTimerStartMethod = assembly.MainModule.ImportReference(executionStatisticsType.Resolve().Methods.First(m => m.Name == "RenderTimerStart"));
                MethodReference renderTimerStopMethod = assembly.MainModule.ImportReference(executionStatisticsType.Resolve().Methods.First(m => m.Name == "RenderTimerStop"));

                //Log.LogMessage(MessageImportance.High, $"ComponentMethodInjector 3");

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

                //Log.LogMessage(MessageImportance.High, $"ComponentMethodInjector 4");

                // Save the modified assembly back to the original file
                assembly.Write(AssemblyPath);

                //Log.LogMessage(MessageImportance.High, $"ComponentMethodInjector 5");
            }
            catch (Exception ex)
            {
                //Log.LogError("Error while inspecting the project: " + ex.Message);
                return false;
            }

            return true;
        }
    }
}