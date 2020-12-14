using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Trigger the job immediately and then after the given timespan expired. 
    /// </summary>
    public class IntervalSchedule : ISchedule, IScheduleWithPrio
    {
        /// <summary>
        /// Create interval schedule with the given interval
        /// </summary>
        /// <param name="interval">interval</param>
        public IntervalSchedule(TimeSpan interval)
        {
            Interval = interval;
        }

        /// <summary>
        /// Interval after an execution attempt (successful or not) when retry is triggered.
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <inheritdoc />
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution,
            IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            if (lastExecution == null)
            {
                return Priority;
            }

            if (lastExecution.ExecutionTime + Interval < now)
            {
                var delay = now - (lastExecution.ExecutionTime + Interval);
                var delayTotalMinutes = (int) delay.TotalMinutes * Priority;
                return Math.Max(delayTotalMinutes, Priority);
            }

            return 0;
        }

        /// <summary>
        /// Custom priority. The priority is multiplied by the minutes of delay to the actual trigger time.
        /// </summary>
        public int Priority { get; set; } = 1;
    }
}