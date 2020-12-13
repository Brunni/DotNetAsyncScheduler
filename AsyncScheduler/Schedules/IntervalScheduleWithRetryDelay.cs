using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Trigger job as in <see cref="IntervalSchedule"/> but retry failed attempts after the retry delay.
    /// </summary>
    public class IntervalScheduleWithRetryDelay : IScheduleWithPrio, ISchedule
    {
        private readonly IntervalSchedule _intervalSchedule;

        /// <summary>
        /// Create interval schedule with the given interval
        /// </summary>
        /// <param name="interval">interval</param>
        public IntervalScheduleWithRetryDelay(TimeSpan interval)
        {
            _intervalSchedule = new IntervalSchedule(interval);
        }

        /// <inheritdoc />
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution, DateTime now)
        {
            var basePriority = _intervalSchedule.GetExecutionPriority(jobKey, lastExecution, lastSuccessfulExecution, now);
            if (basePriority > 0)
            {
                return basePriority;
            }
            if (lastExecution.JobResult == JobResult.Failure)
            {
                // Don't retry immediately
                if (lastExecution.ExecutionTime + RetryDelay > now)
                {
                    return 0;
                }

                return Priority;
            }

            return basePriority;
        }

        /// <summary>
        /// Delay for retry after failed execution.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <inheritdoc />
        public int Priority
        {
            get => _intervalSchedule.Priority;
            set => _intervalSchedule.Priority = value;
        }
    }
}