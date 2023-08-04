using System;
using System.Diagnostics;

namespace BlazorWasmProfiler
{
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

                TotalTime += GetElapsedTime(_startTime);

                Count++;
            }
        }

        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private static readonly double s_tickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;
        private static TimeSpan GetElapsedTime(long startingTimestamp) => GetElapsedTime(startingTimestamp, Stopwatch.GetTimestamp());
        private static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) => new TimeSpan((long)((endingTimestamp - startingTimestamp) * s_tickFrequency));
    }
}