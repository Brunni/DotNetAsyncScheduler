using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Job is never executed automatically. But can be started by <see cref="Scheduler.QuickStartQueue"/> manually.
    /// </summary>
    public class ScheduleNever : ISchedule
    {
        /// <inheritdoc />
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution, DateTime now)
        {
            return 0;
        }
    }
}
