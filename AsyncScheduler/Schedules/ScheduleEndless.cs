using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Always run the task (when it is not running anymore).
    /// </summary>
    public class ScheduleEndless : ISchedule, IScheduleWithPrio
    {
        /// <inheritdoc />
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            return Priority;
        }

        /// <inheritdoc />
        public int Priority { get; set; } = 1;
    }
}