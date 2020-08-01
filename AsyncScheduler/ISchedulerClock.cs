using System;

namespace AsyncScheduler
{
    /// <summary>
    /// Interface to allow mocking the clock for unit tests
    /// </summary>
    public interface ISchedulerClock
    {
        DateTime GetNow();
    }

    /// <summary>
    /// Default implementation of clock
    /// </summary>
    internal class UtcSchedulerClock : ISchedulerClock
    {
        public DateTime GetNow()
        {
            return DateTime.UtcNow;
        }
    }
}