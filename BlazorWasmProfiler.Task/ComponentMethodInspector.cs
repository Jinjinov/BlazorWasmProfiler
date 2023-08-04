using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System;

namespace BlazorWasmProfiler
{
    public class ComponentMethodInspector : Task
    {
        [Required]
        public string AssemblyPath { get; set; } = string.Empty;

        public override bool Execute()
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