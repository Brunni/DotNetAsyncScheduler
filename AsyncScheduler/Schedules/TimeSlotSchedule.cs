using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Job is executed once in the given time slot.
    /// On failure task is immediately restarted.
    /// A similar implementation could be done, where the StartTime is automatically fetched from Database or Filesystem.
    /// </summary>
    public class TimeSlotSchedule : ISchedule
    {
        /// <summary>
        /// Earliest time for the task to start.
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// If task has not been successful executed in this time span, it is not triggered.
        /// </summary>
        /// <remarks>Start could be prohibited by restrictions</remarks>
        public TimeSpan SlotTime { get; set; } = TimeSpan.FromMinutes(10);
        
        /// <inheritdoc />
        public int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution, IJobHistoryEntry lastSuccessfulExecution,
            DateTime now)
        {
            var lastExecutionTime = lastSuccessfulExecution?.ExecutionTime ?? DateTime.MinValue; 
            if (now > StartTime && lastExecutionTime < StartTime && now < StartTime + SlotTime)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}