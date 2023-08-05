using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

namespace BlazorWasmProfiler.Weaver;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
            return;

        string assemblyPath = args.First();

        if (!File.Exists(assemblyPath))
            return;

        try
        {
            Console.WriteLine($"ComponentMethodInjector 1 {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"ComponentMethodInjector 1 {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");

            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

            Console.WriteLine($"ComponentMethodInjector 2 {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"ComponentMethodInjector 2 {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");

            // Load ExecutionStatistics type and methods
            TypeReference executionStatisticsType = assembly.MainModule.ImportReference(typeof(ExecutionStatistics));
            MethodReference renderTimerStartMethod = assembly.MainModule.ImportReference(executionStatisticsType.Resolve().Methods.First(m => m.Name == "RenderTimerStart"));
            MethodReference renderTimerStopMethod = assembly.MainModule.ImportReference(executionStatisticsType.Resolve().Methods.First(m => m.Name == "RenderTimerStop"));

            Console.WriteLine($"ComponentMethodInjector 3");

            foreach (ModuleDefinition module in assembly.Modules)
            {
                Console.WriteLine($"ComponentMethodInjector 4");

                foreach (TypeDefinition type in module.Types)
                {
                    Console.WriteLine($"ComponentMethodInjector 5");

                    if (type.BaseType?.FullName == "Microsoft.AspNetCore.Components.ComponentBase")
                    {
                        Console.WriteLine($"ComponentMethodInjector 6");

                        foreach (MethodDefinition method in type.Methods)
                        {
                            Console.WriteLine($"ComponentMethodInjector 7");

                            if ((method.Name == "OnParametersSet" || method.Name == "OnAfterRender") &&
                                method.IsFamily &&
                                method.IsVirtual &&
                                method.ReturnType.FullName == "System.Void")
                            {
                                ILProcessor ilProcessor = method.Body.GetILProcessor();

                                Console.WriteLine($"ComponentMethodInjector 8");

                                if (method.Name == "OnAfterRender")
                                {
                                    Console.WriteLine($"ComponentMethodInjector 9");

                                    Instruction firstInstruction = method.Body.Instructions[0];
                                    ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, type.FullName));
                                    ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, renderTimerStopMethod));
                                }

                                if (method.Name == "OnParametersSet")
                                {
                                    Console.WriteLine($"ComponentMethodInjector 0");

                                    Instruction lastInstruction = method.Body.Instructions[method.Body.Instructions.Count - 1];
                                    ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Ldstr, type.FullName));
                                    ilProcessor.InsertBefore(lastInstruction, ilProcessor.Create(OpCodes.Call, renderTimerStartMethod));
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"ComponentMethodInjector 11");

            // Save the modified assembly back to the original file
            assembly.Write(assemblyPath);

            Console.WriteLine($"ComponentMethodInjector 12");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while inspecting the project: " + ex.Message);
        }
    }
}
