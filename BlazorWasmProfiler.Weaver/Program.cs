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

            string tempPath = assemblyPath + ".temp";

            // Make a copy of the original assembly
            File.Copy(assemblyPath, tempPath, true);

            // Load the copied assembly
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(tempPath);

            //AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

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
                        Console.WriteLine($"ComponentMethodInjector 6 {type.FullName}");

                        foreach (MethodDefinition method in type.Methods)
                        {
                            Console.WriteLine($"ComponentMethodInjector 7 {method.Name} {method.IsFamily} {method.IsVirtual} {method.ReturnType}");

                            if ((method.Name == "OnParametersSet" || method.Name == "OnAfterRender") &&
                                method.IsFamily &&
                                method.IsVirtual &&
                                method.ReturnType.FullName == "System.Void")
                            {
                                Console.WriteLine($"ComponentMethodInjector 7.1");

                                MethodDefinition methodToModify = method;

                                bool isOverride = method.IsVirtual && method.IsReuseSlot && method.DeclaringType != type;

                                if (!isOverride)
                                {
                                    Console.WriteLine($"ComponentMethodInjector 7.2");

                                    // Step 4: Add an override of VirtualMethod
                                    MethodDefinition overrideMethod = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                                    overrideMethod.IsVirtual = true;
                                    overrideMethod.IsReuseSlot = true;
                                    overrideMethod.Overrides.Add(new MethodReference(method.Name, method.ReturnType, type));

                                    // Optional: Copy the parameters from the original VirtualMethod
                                    foreach (var parameter in method.Parameters)
                                    {
                                        overrideMethod.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType));
                                    }

                                    // Step 5: Add the new method to MyType
                                    type.Methods.Add(overrideMethod);

                                    methodToModify = overrideMethod;
                                }

                                ILProcessor ilProcessor = methodToModify.Body.GetILProcessor();

                                Console.WriteLine($"ComponentMethodInjector 8");

                                if (methodToModify.Name == "OnAfterRender")
                                {
                                    Console.WriteLine($"ComponentMethodInjector 9");

                                    Instruction firstInstruction = methodToModify.Body.Instructions[0];
                                    ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldstr, type.FullName));
                                    ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, renderTimerStopMethod));
                                }

                                if (methodToModify.Name == "OnParametersSet")
                                {
                                    Console.WriteLine($"ComponentMethodInjector 0");

                                    Instruction lastInstruction = methodToModify.Body.Instructions[^1];
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
            //assembly.Write(assemblyPath);

            int maxRetries = 100;
            int retryDelayMs = 200; // Adjust this value if needed

            // Try writing the modified assembly back, retry if the file is locked
            bool success = false;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    assembly.Write(assemblyPath);
                    success = true;
                    break;
                }
                catch (IOException ex)
                {
                    // If the file is locked, wait for a while and try again
                    Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}");
                    System.Threading.Thread.Sleep(retryDelayMs);
                }
            }

            if (success)
            {
                Console.WriteLine("Assembly modification succeeded!");
            }
            else
            {
                Console.WriteLine("Assembly modification failed after multiple retries.");
            }

            assembly.Dispose();

            // Clean up the temporary file
            File.Delete(tempPath);

            Console.WriteLine($"ComponentMethodInjector 12");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while inspecting the project: " + ex.Message);
        }
    }
}
