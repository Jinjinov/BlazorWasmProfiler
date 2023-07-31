using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace BlazorWasmProfiler;

[Aspect(Scope.Global)]
[Injection(typeof(BlazorTimerAttribute))]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class BlazorTimerAttribute : Attribute
{
    private static readonly Dictionary<string, ExecutionStatistics> _methodStatistics = new();
    private static readonly Dictionary<string, ExecutionStatistics> _renderStatistics = new();

    public static IReadOnlyDictionary<string, ExecutionStatistics> GetMethodStatistics() => _methodStatistics;
    public static IReadOnlyDictionary<string, ExecutionStatistics> GetRenderStatistics() => _renderStatistics;

    [Advice(Kind.Before)]
    public void OnEntry([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
    {
        string declaringTypeName = declaringType.FullName ?? string.Empty;
        string methodFullName = $"{declaringTypeName}.{methodName}";

        (string callerClassName, string callerMethodName) = GetCallerMethodName();
        string callerFullName = $"{callerClassName}.{callerMethodName}";

        string methodKey = $"{callerFullName}-{methodFullName}";

        if (!_methodStatistics.TryGetValue(methodKey, out var methodStatistics))
        {
            methodStatistics = new ExecutionStatistics() { MethodName = methodFullName, CallerMethodName = callerFullName };
            _methodStatistics[methodKey] = methodStatistics;
        }

        methodStatistics.StartTiming();

        if (methodName == "OnParametersSet")
        {
            string renderKey = declaringTypeName;

            if (!_renderStatistics.TryGetValue(renderKey, out var renderStatistics))
            {
                renderStatistics = new ExecutionStatistics() { MethodName = declaringTypeName, CallerMethodName = callerClassName };
                _renderStatistics[renderKey] = renderStatistics;
            }

            renderStatistics.StartTiming();
        }
    }

    [Advice(Kind.After)]
    public void OnExit([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
    {
        string declaringTypeName = declaringType.FullName ?? string.Empty;
        string methodFullName = $"{declaringTypeName}.{methodName}";

        (string callerClassName, string callerMethodName) = GetCallerMethodName();
        string callerFullName = $"{callerClassName}.{callerMethodName}";

        string methodKey = $"{callerFullName}-{methodFullName}";

        if (_methodStatistics.TryGetValue(methodKey, out var methodStatistics))
        {
            methodStatistics.StopTiming();
        }

        if (methodName == "OnAfterRender")
        {
            string renderKey = declaringTypeName;

            if (_renderStatistics.TryGetValue(renderKey, out var renderStatistics))
            {
                renderStatistics.StopTiming();
            }
        }
    }

#pragma warning disable CA1822 // Mark members as static
    private (string callerClassName, string callerMethodName) GetCallerMethodName()
#pragma warning restore CA1822 // Mark members as static
    {
        StackTrace stackTrace = new();

        if (stackTrace.GetFrame(3) is StackFrame callerFrame && callerFrame.GetMethod() is MethodBase method)
        {
            string callerMethodName = method.Name;
            string callerClassName = method.DeclaringType?.FullName ?? string.Empty;

            return (callerClassName, callerMethodName);
        }

        return (string.Empty, string.Empty);
    }
}
