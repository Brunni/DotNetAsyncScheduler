using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Always run the task (when it is not running anymore).
    /// </summary>
    public class ScheduleEndless : ISchedule
    {
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            return 1;
        }
    }
}