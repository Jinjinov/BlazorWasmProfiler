using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlazorWasmProfiler;

[Aspect(Scope.Global)]
[Injection(typeof(MethodExecutionTimeAttribute))]
public class MethodExecutionTimeAttribute : Attribute
{
    private static readonly Dictionary<string, ExecutionStatistics> _methodStatistics = new();

    public static IReadOnlyDictionary<string, ExecutionStatistics> GetMethodStatistics() => _methodStatistics;

    [Advice(Kind.Before)]
    public void OnEntry([Argument(Source.Name)] string methodName)
    {
        string callerMethodName = GetCallerMethodName();
        string key = $"{callerMethodName}-{methodName}";

        if (!_methodStatistics.ContainsKey(key))
        {
            _methodStatistics[key] = new ExecutionStatistics() { MethodName = methodName, CallerMethodName = callerMethodName };
        }

        _methodStatistics[key].StartTiming();
    }

    [Advice(Kind.After)]
    public void OnExit([Argument(Source.Name)] string methodName)
    {
        string callerMethodName = GetCallerMethodName();
        string key = $"{callerMethodName}-{methodName}";

        if (_methodStatistics.ContainsKey(key))
        {
            _methodStatistics[key].StopTiming();
        }
    }

    private string GetCallerMethodName()
    {
        StackTrace stackTrace = new();

        if (stackTrace.FrameCount > 2)
        {
            StackFrame callerFrame = stackTrace.GetFrame(2); // Get the frame at the 2nd previous level in the call stack.
            string callerMethodName = callerFrame.GetMethod().Name;
            string callerClassName = callerFrame.GetMethod().DeclaringType?.FullName;
            return $"{callerClassName}.{callerMethodName}";
        }

        return "Unknown";
    }
}
