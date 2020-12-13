using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Simple schedule to execute task once without delay. On failure no rescheduling.
    /// </summary>
    public class ScheduleOnce : ISchedule, IScheduleWithPrio
    {
        /// <inheritdoc />
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            return lastExecution == null ? Priority : 0;
        }

        /// <inheritdoc />
        public int Priority { get; set; } = 1;
    }
}