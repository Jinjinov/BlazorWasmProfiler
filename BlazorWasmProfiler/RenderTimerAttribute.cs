using AspectInjector.Broker;
using System;

namespace BlazorWasmProfiler
{
    [Aspect(Scope.Global)]
    [Injection(typeof(RenderTimerAttribute))]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RenderTimerAttribute : Attribute
    {
        [Advice(Kind.Before)]
        public void OnEntry([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
        {
            if (methodName == "OnAfterRender" || methodName == "OnAfterRenderAsync")
            {
                ExecutionStatistics.RenderTimerStop(declaringType.FullName ?? string.Empty);
            }
        }

        [Advice(Kind.After)]
        public void OnExit([Argument(Source.Name)] string methodName, [Argument(Source.Type)] Type declaringType)
        {
            if (methodName == "OnParametersSet" || methodName == "OnParametersSetAsync")
            {
                ExecutionStatistics.RenderTimerStart(declaringType.FullName ?? string.Empty);
            }
        }
    }
}