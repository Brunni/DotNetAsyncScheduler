using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Job is never executed automatically. But can be started by <see cref="Scheduler.QuickStartQueue"/> manually.
    /// </summary>
    public class ScheduleNever : ISchedule
    {
        /// <inheritdoc />
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry? lastExecution, IJobHistoryEntry? lastSuccessfulExecution, DateTimeOffset now)
        {
            return 0;
        }
    }
}
