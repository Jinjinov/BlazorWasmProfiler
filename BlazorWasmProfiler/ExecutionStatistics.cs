using System;
using System.Diagnostics;

namespace BlazorWasmProfiler;

[Serializable]
public class ExecutionStatistics
{
    private long _startTime;

    public string MethodName { get; set; } = string.Empty;
    public string CallerMethodName { get; set; } = string.Empty;

    public int Count { get; set; }
    public TimeSpan TotalTime { get; set; }

    public TimeSpan GetAverageTime() => Count == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(TotalTime.Ticks / Count);

    public void StartTiming()
    {
        _startTime = Stopwatch.GetTimestamp();
    }

    public void StopTiming()
    {
        TotalTime += Stopwatch.GetElapsedTime(_startTime);

        Count++;
    }
}
