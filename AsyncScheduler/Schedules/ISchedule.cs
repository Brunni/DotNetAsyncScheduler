using System;
using AsyncScheduler.History;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Schedule is instantiated on each check run and the method is called.
    /// Schedule could access database/file to get current settings for the schedule.
    /// Method could be called quite often, depending on the configured delay.
    /// </summary>
    public interface ISchedule
    {
        /// <summary>
        /// Check if job should be executed by the schedule.
        /// The higher the number, the higher the priority.
        /// </summary>
        /// <remarks>Priority only plays a role, if restrictions are applied.</remarks>
        /// <returns>0 = don't execute</returns>
        int GetExecutionPriority(string jobKey, IJobHistoryEntry lastExecution,
            IJobHistoryEntry lastSuccessfulExecution, DateTime now);
    }
}