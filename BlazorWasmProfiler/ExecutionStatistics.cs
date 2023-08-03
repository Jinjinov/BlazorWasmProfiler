using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace BlazorWasmProfiler;

public static class ExecutionStatistics
{
    private static readonly Dictionary<string, ExecutionData> _methodStatistics = new();
    private static readonly Dictionary<string, ExecutionData> _renderStatistics = new();

    public static IReadOnlyDictionary<string, ExecutionData> GetMethodStatistics() => _methodStatistics;
    public static IReadOnlyDictionary<string, ExecutionData> GetRenderStatistics() => _renderStatistics;

    public static IEnumerable<ExecutionData> GetMethodStatistics(StatisticsOrder order) => OrderStatisticsBy(_methodStatistics.Values, order);
    public static IEnumerable<ExecutionData> GetRenderStatistics(StatisticsOrder order) => OrderStatisticsBy(_renderStatistics.Values, order);

    public static void OnEntry(string methodName, Type declaringType)
    {
        string declaringTypeName = declaringType.FullName ?? string.Empty;
        string methodFullName = $"{declaringTypeName}.{methodName}";

        (string callerClassName, string callerMethodName) = GetCallerMethodName();
        string callerFullName = $"{callerClassName}.{callerMethodName}";

        string methodKey = $"{callerFullName}-{methodFullName}";

        if (!_methodStatistics.TryGetValue(methodKey, out var methodStatistics))
        {
            methodStatistics = new ExecutionData() { Name = methodFullName, Caller = callerFullName };
            _methodStatistics[methodKey] = methodStatistics;
        }

        methodStatistics.StartTiming();

        if (methodName == "OnAfterRender" || methodName == "OnAfterRenderAsync")
        {
            string renderKey = declaringTypeName;

            if (_renderStatistics.TryGetValue(renderKey, out var renderStatistics))
            {
                renderStatistics.StopTiming();
            }
        }
    }

    public static void OnExit(string methodName, Type declaringType)
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

        if (methodName == "OnParametersSet" || methodName == "OnParametersSetAsync")
        {
            string renderKey = declaringTypeName;

            if (!_renderStatistics.TryGetValue(renderKey, out var renderStatistics))
            {
                renderStatistics = new ExecutionData() { Name = declaringTypeName, Caller = callerClassName };
                _renderStatistics[renderKey] = renderStatistics;
            }

            renderStatistics.StartTiming();
        }
    }

    private static (string callerClassName, string callerMethodName) GetCallerMethodName()
    {
        StackTrace stackTrace = new();

        if (stackTrace.GetFrame(4) is StackFrame callerFrame && callerFrame.GetMethod() is MethodBase method)
        {
            string callerMethodName = method.Name;
            string callerClassName = method.DeclaringType?.FullName ?? string.Empty;

            return (callerClassName, callerMethodName);
        }

        return (string.Empty, string.Empty);
    }

    private static IEnumerable<ExecutionData> OrderStatisticsBy(IEnumerable<ExecutionData> statistics, StatisticsOrder order)
    {
        return order switch
        {
            StatisticsOrder.Caller => statistics.OrderBy(stat => stat.Caller),
            StatisticsOrder.Name => statistics.OrderBy(stat => stat.Name),
            StatisticsOrder.Count => statistics.OrderByDescending(stat => stat.Count),
            StatisticsOrder.TotalTime => statistics.OrderByDescending(stat => stat.TotalTime.TotalMilliseconds),
            StatisticsOrder.AverageTime => statistics.OrderByDescending(stat => stat.GetAverageTime().TotalMilliseconds),
            _ => throw new ArgumentException("Invalid StatisticsOrder value.", nameof(order)),
        };
    }
}
