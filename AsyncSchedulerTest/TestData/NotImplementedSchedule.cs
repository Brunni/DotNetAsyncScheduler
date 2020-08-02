using System;
using AsyncScheduler.History;
using AsyncScheduler.Schedules;

namespace AsyncSchedulerTest.TestData
{
    public class NotImplementedSchedule : ISchedule
    {
        public string Marker { get; }

        public NotImplementedSchedule(string marker)
        {
            Marker = marker;
        }

        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            throw new NotImplementedException();
        }
    }
}