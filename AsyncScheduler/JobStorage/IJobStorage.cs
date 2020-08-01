using System;
using System.Collections.Generic;
using AsyncScheduler.Schedules;

namespace AsyncScheduler.JobStorage
{
    /// <summary>
    /// Interface to storing the currently registered jobs with their schedules.
    /// </summary>
    public interface IJobStorage
    {
        /// <summary>
        /// Combined method for adding, updating or adding+updating a job.
        /// </summary>
        /// <param name="scheduleProvider">schedule</param>
        /// <param name="update">true, job should be updated, if it exists</param>
        /// <param name="add">true, job should be added, if it does not exist</param>
        /// <typeparam name="TJob">Job Type</typeparam>
        void AddOrUpdateJobInternal<TJob>(IScheduleProvider scheduleProvider, bool update = false,
            bool add = true) where TJob : IJob;

        /// <summary>
        /// Access to Jobs.
        /// </summary>
        IDictionary<string, Type> Jobs { get; }
        
        /// <summary>
        /// Access to Schedules
        /// </summary>
        IDictionary<string, IScheduleProvider> Schedules { get; }
        
        /// <summary>
        /// Remove job
        /// </summary>
        /// <param name="jobKey">job key</param>
        void Remove(string jobKey);
    }
}