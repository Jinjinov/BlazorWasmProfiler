using AspectInjector.Broker;
using System;

namespace BlazorWasmProfiler
{
    [Aspect(Scope.Global)]
    [Injection(typeof(MethodTimerAttribute))]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MethodTimerAttribute : Attribute
    {
        [Advice(Kind.Before)]
        public void OnEntry([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
        {
            ExecutionStatistics.MethodTimerStart(methodName, declaringType.FullName ?? string.Empty);
        }

        [Advice(Kind.After)]
        public void OnExit([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
        {
            ExecutionStatistics.MethodTimerStop(methodName, declaringType.FullName ?? string.Empty);
        }
    }
}