using System;

namespace AsyncScheduler
{
    /// <summary>
    /// Interface to allow mocking the clock for unit tests
    /// </summary>
    public interface ISchedulerClock
    {
        /// <summary>
        /// Returns now
        /// </summary>
        DateTimeOffset GetNow();
    }

    /// <summary>
    /// Default implementation of clock
    /// </summary>
    internal class UtcSchedulerClock : ISchedulerClock
    {
        public DateTimeOffset GetNow()
        {
            return DateTimeOffset.Now;
        }
    }
}