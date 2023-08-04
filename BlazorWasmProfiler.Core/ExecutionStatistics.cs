using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace BlazorWasmProfiler
{
    public static class ExecutionStatistics
    {
        private static readonly Dictionary<string, ExecutionData> _methodStatistics = new Dictionary<string, ExecutionData>();
        private static readonly Dictionary<string, ExecutionData> _renderStatistics = new Dictionary<string, ExecutionData>();

        public static IReadOnlyDictionary<string, ExecutionData> GetMethodStatistics() => _methodStatistics;
        public static IReadOnlyDictionary<string, ExecutionData> GetRenderStatistics() => _renderStatistics;

        public static IEnumerable<ExecutionData> GetMethodStatistics(StatisticsOrder order) => OrderStatisticsBy(_methodStatistics.Values, order);
        public static IEnumerable<ExecutionData> GetRenderStatistics(StatisticsOrder order) => OrderStatisticsBy(_renderStatistics.Values, order);

        public static void MethodTimerStart(string methodName, string declaringTypeName)
        {
            string methodFullName = $"{declaringTypeName}.{methodName}";

            (string callerClassName, string callerMethodName) = GetCallerMethodName();
            string callerFullName = $"{callerClassName}.{callerMethodName}";

            string methodKey = $"{callerFullName}-{methodFullName}";

            if (!_methodStatistics.TryGetValue(methodKey, out ExecutionData methodStatistics))
            {
                methodStatistics = new ExecutionData() { Name = methodFullName, Caller = callerFullName };
                _methodStatistics[methodKey] = methodStatistics;
            }

            methodStatistics.StartTiming();
        }

        public static void RenderTimerStop(string declaringTypeName)
        {
            if (_renderStatistics.TryGetValue(declaringTypeName, out ExecutionData renderStatistics))
            {
                renderStatistics.StopTiming();
            }
        }

        public static void MethodTimerStop(string methodName, string declaringTypeName)
        {
            string methodFullName = $"{declaringTypeName}.{methodName}";

            (string callerClassName, string callerMethodName) = GetCallerMethodName();
            string callerFullName = $"{callerClassName}.{callerMethodName}";

            string methodKey = $"{callerFullName}-{methodFullName}";

            if (_methodStatistics.TryGetValue(methodKey, out ExecutionData methodStatistics))
            {
                methodStatistics.StopTiming();
            }
        }

        public static void RenderTimerStart(string declaringTypeName)
        {
            (string callerClassName, string callerMethodName) = GetCallerMethodName();

            if (!_renderStatistics.TryGetValue(declaringTypeName, out ExecutionData renderStatistics))
            {
                renderStatistics = new ExecutionData() { Name = declaringTypeName, Caller = callerClassName };
                _renderStatistics[declaringTypeName] = renderStatistics;
            }

            renderStatistics.StartTiming();
        }

        private static (string callerClassName, string callerMethodName) GetCallerMethodName()
        {
            StackTrace stackTrace = new StackTrace();

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
}