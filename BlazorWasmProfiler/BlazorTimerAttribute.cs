using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    public static IEnumerable<ExecutionStatistics> GetMethodStatistics(StatisticsOrder order) => OrderStatisticsBy(_methodStatistics.Values, order);
    public static IEnumerable<ExecutionStatistics> GetRenderStatistics(StatisticsOrder order) => OrderStatisticsBy(_renderStatistics.Values, order);

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
            methodStatistics = new ExecutionStatistics() { Name = methodFullName, Caller = callerFullName };
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

        if (methodName == "OnParametersSet" || methodName == "OnParametersSetAsync")
        {
            string renderKey = declaringTypeName;

            if (!_renderStatistics.TryGetValue(renderKey, out var renderStatistics))
            {
                renderStatistics = new ExecutionStatistics() { Name = declaringTypeName, Caller = callerClassName };
                _renderStatistics[renderKey] = renderStatistics;
            }

            renderStatistics.StartTiming();
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

    private static IEnumerable<ExecutionStatistics> OrderStatisticsBy(IEnumerable<ExecutionStatistics> statistics, StatisticsOrder order)
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
