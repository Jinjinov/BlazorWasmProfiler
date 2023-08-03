using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System;

namespace BlazorWasmProfiler;

public class ComponentMethodInjector : Task
{
    [Required]
    public string AssemblyPath { get; set; }

    public override bool Execute()
    {
        try
        {
            var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath);

            int derivedFromClassBaseCount = 0;
            int onParametersSetCount = 0;

            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (type.BaseType?.FullName == "Microsoft.AspNetCore.Components.ComponentBase")
                    {
                        derivedFromClassBaseCount++;

                        foreach (var method in type.Methods)
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