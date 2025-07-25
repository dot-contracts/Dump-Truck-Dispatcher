using System;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class GetElapsedTimeResult
    {
        public bool ClockIsStarted { get; set; }

        public DateTime? LastClockStartTimeUtc { get; set; }

        public double CommittedElapsedSeconds { get; set; }

        public DateTime CommittedElapsedSecondsForDay { get; set; }
    }
}
