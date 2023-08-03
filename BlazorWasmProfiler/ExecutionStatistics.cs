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

    public static void MethodTimerStart(string methodName, Type declaringType)
    {
        Names names = GetNames(methodName, declaringType);

        if (!_methodStatistics.TryGetValue(names.MethodKey, out var methodStatistics))
        {
            methodStatistics = new ExecutionData() { Name = names.MethodFullName, Caller = names.CallerFullName };
            _methodStatistics[names.MethodKey] = methodStatistics;
        }

        methodStatistics.StartTiming();
    }

    public static void RenderTimerStop(string methodName, Type declaringType)
    {
        Names names = GetNames(methodName, declaringType);

        string renderKey = names.DeclaringTypeName;

        if (_renderStatistics.TryGetValue(renderKey, out var renderStatistics))
        {
            renderStatistics.StopTiming();
        }
    }

    public static void MethodTimerStop(string methodName, Type declaringType)
    {
        Names names = GetNames(methodName, declaringType);

        if (_methodStatistics.TryGetValue(names.MethodKey, out var methodStatistics))
        {
            methodStatistics.StopTiming();
        }
    }

    public static void RenderTimerStart(string methodName, Type declaringType)
    {
        Names names = GetNames(methodName, declaringType);

        string renderKey = names.DeclaringTypeName;

        if (!_renderStatistics.TryGetValue(renderKey, out var renderStatistics))
        {
            renderStatistics = new ExecutionData() { Name = names.DeclaringTypeName, Caller = names.CallerClassName };
            _renderStatistics[renderKey] = renderStatistics;
        }

        renderStatistics.StartTiming();
    }

    public static Names GetNames(string methodName, Type declaringType)
    {
        string callerClassName = string.Empty;
        string callerMethodName = string.Empty;

        StackTrace stackTrace = new();

        if (stackTrace.GetFrame(3) is StackFrame callerFrame && callerFrame.GetMethod() is MethodBase method)
        {
            callerClassName = method.DeclaringType?.FullName ?? string.Empty;
            callerMethodName = method.Name;
        }

        return new Names(methodName, declaringType.FullName ?? string.Empty, callerClassName, callerMethodName);
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
