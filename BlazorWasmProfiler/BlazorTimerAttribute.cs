﻿using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace BlazorWasmProfiler;

[Aspect(Scope.Global)]
[Injection(typeof(BlazorTimerAttribute))]
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
        string callerMethodName = GetCallerMethodName();
        string methodKey = $"{callerMethodName}-{declaringTypeName}.{methodName}";

        if (!_methodStatistics.TryGetValue(methodKey, out var methodStatistics))
        {
            methodStatistics = new ExecutionStatistics() { MethodName = $"{declaringTypeName}.{methodName}", CallerMethodName = callerMethodName };
            _methodStatistics[methodKey] = methodStatistics;
        }

        methodStatistics.StartTiming();

        if (methodName == "OnParametersSet")
        {
            string renderKey = declaringTypeName;

            if (!_renderStatistics.TryGetValue(renderKey, out var renderStatistics))
            {
                renderStatistics = new ExecutionStatistics() { MethodName = $"{declaringTypeName}.{methodName}", CallerMethodName = callerMethodName };
                _renderStatistics[renderKey] = renderStatistics;
            }

            renderStatistics.StartTiming();
        }
    }

    [Advice(Kind.After)]
    public void OnExit([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
    {
        string declaringTypeName = declaringType.FullName ?? string.Empty;
        string callerMethodName = GetCallerMethodName();
        string methodKey = $"{callerMethodName}-{declaringTypeName}.{methodName}";

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

    private string GetCallerMethodName()
    {
        StackTrace stackTrace = new();

        if (stackTrace.GetFrame(3) is StackFrame callerFrame && callerFrame.GetMethod() is MethodBase method)
        {
            string callerMethodName = method.Name;
            string callerClassName = method.DeclaringType?.FullName ?? string.Empty;

            return $"{callerClassName}.{callerMethodName}";
        }

        return string.Empty;
    }
}
