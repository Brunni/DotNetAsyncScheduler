using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    public class IntervalSchedule : ISchedule
    {
        public IntervalSchedule(TimeSpan interval)
        {
            Interval = interval;
        }

        public TimeSpan Interval { get; set; }

        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution,
            IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            if (lastExecution == null)
            {
                return 1;
            }

            if (lastExecution.ExecutionTime + Interval < now)
            {
                var delay = now - lastExecution.ExecutionTime + Interval;
                var delayTotalMinutes = (int) delay.TotalMinutes;
                return Math.Max(delayTotalMinutes, 1);
            }

            return 0;
        }
    }
}