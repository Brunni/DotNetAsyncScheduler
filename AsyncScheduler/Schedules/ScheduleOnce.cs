using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Simple schedule to execute task once without delay. On failure no rescheduling.
    /// </summary>
    public class ScheduleOnce : ISchedule
    {
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            return lastExecution == null ? 1 : 0;
        }
    }
}