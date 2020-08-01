using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AsyncScheduler.Schedules;

namespace AsyncScheduler.JobStorage
{
    public class InMemoryStorage : IJobStorage
    {
        private readonly ConcurrentDictionary<string, Type> _jobs = new ConcurrentDictionary<string, Type>();

        private readonly ConcurrentDictionary<string, IScheduleProvider> _schedules =
            new ConcurrentDictionary<string, IScheduleProvider>();

        public IDictionary<string, Type> Jobs => _jobs;

        public IDictionary<string, IScheduleProvider> Schedules => _schedules;


        public void AddOrUpdateJobInternal<TJob>(IScheduleProvider scheduleProvider, bool update = false,
            bool add = true) where TJob : IJob
        {
            if (scheduleProvider == null) throw new ArgumentNullException(nameof(scheduleProvider));
            var jobType = typeof(TJob);
            var jobKey = jobType.FullName ?? throw new NullReferenceException("jobKey");
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

        public void Remove(string jobKey)
        {
            _jobs.TryRemove(jobKey, out _);
            _schedules.TryRemove(jobKey, out _);
        }
    }
}