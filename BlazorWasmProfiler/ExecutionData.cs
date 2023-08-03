using System;
using System.Diagnostics;

namespace BlazorWasmProfiler;

public class ExecutionData
{
    private long _startTime;
    private bool _isTiming;

    public string Name { get; set; } = string.Empty;
    public string Caller { get; set; } = string.Empty;

    public int Count { get; set; }
    public TimeSpan TotalTime { get; set; }

    public TimeSpan GetAverageTime() => Count == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(TotalTime.Ticks / Count);

    public void StartTiming()
    {
        _isTiming = true;

        _startTime = Stopwatch.GetTimestamp();
    }

    public void StopTiming()
    {
        if (_isTiming)
        {
            _isTiming = false;

            TotalTime += Stopwatch.GetElapsedTime(_startTime);

            Count++;
        }
    }
}
