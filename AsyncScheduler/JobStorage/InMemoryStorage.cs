using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AsyncScheduler.Schedules;

namespace AsyncScheduler.JobStorage
{
    /// <summary>
    /// Default storage for holding the Jobs, Schedules etc.
    /// It just holds the information in memory.
    /// </summary>
    public class InMemoryStorage : IJobStorage
    {
        private readonly ConcurrentDictionary<string, Type> _jobs = new();

        private readonly ConcurrentDictionary<string, IScheduleProvider> _schedules = new();

        /// <inheritdoc />
        public IDictionary<string, Type> Jobs => _jobs;

        /// <inheritdoc />
        public IDictionary<string, IScheduleProvider> Schedules => _schedules;


        /// <inheritdoc />
        public void AddOrUpdateJobInternal<TJob>(IScheduleProvider scheduleProvider, bool update = false,
            bool add = true) where TJob : IJob
        {
            if (scheduleProvider == null) throw new ArgumentNullException(nameof(scheduleProvider));
            var jobType = typeof(TJob);
            string jobKey = jobType.FullName ?? throw new NullReferenceException("jobKey");
            if (!add && !_jobs.ContainsKey(jobKey))
            {
                throw new Exception($"No job {jobKey} found for updating");
            }

            if (!update && _jobs.ContainsKey(jobKey))
            {
                throw new Exception($"Job {jobKey} already exists");
            }

            _jobs[jobKey] = jobType;
            _schedules[jobKey] = scheduleProvider;
        }

        /// <inheritdoc />
        public bool Remove(string jobKey)
        {
            bool removed = _jobs.TryRemove(jobKey, out _);
            if (!removed)
            {
                return false;
            }

            if (!_schedules.TryRemove(jobKey, out _))
            {
                throw new InvalidOperationException($"Major implementation error during removing of job schedule for {jobKey}");
            }

            return true;
        }
    }
}