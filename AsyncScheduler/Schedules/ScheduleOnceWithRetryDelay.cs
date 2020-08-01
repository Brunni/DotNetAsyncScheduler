using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Schedule which executes task once without delay.
    /// On failed execution, execution is retried again and again with <see cref="RetryDelay"/>.
    /// </summary>
    public class ScheduleOnceWithRetryDelay : ISchedule
    {
        /// <summary>
        /// Delay for retry after failed execution.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <inheritdoc />
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            if (lastSuccessfulExecution == null) {
                //Hint: Delayed retry
                if (lastExecution != null && lastExecution.ExecutionTime + RetryDelay > now)
                {
                    return 0;
                }
                return 1;
            }
            else
                return 0;
        }
    }
}