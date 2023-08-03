using AspectInjector.Broker;
using System;

namespace BlazorWasmProfiler;

[Aspect(Scope.Global)]
[Injection(typeof(BlazorTimerAttribute))]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class BlazorTimerAttribute : Attribute
{
    [Advice(Kind.Before)]
    public void OnEntry([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
    {
        ExecutionStatistics.OnEntry(methodName, declaringType);
    }

    [Advice(Kind.After)]
    public void OnExit([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
    {
        ExecutionStatistics.OnExit(methodName, declaringType);
    }
}
