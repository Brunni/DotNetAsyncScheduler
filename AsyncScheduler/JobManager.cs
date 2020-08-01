using System;
using System.Collections.ObjectModel;
using AsyncScheduler.JobStorage;
using AsyncScheduler.Schedules;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace AsyncScheduler
{
    /// <summary>
    /// Holds the <see cref="IJobStorage"/> and provides an interface to add, update and remove jobs.
    /// Jobs can be changed during runtime of the scheduler. (Current executed jobs are not affected)
    /// </summary>
    public class JobManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobManager> _logger;
        private readonly IJobStorage _jobStorage;

        public JobManager(IServiceProvider serviceProvider, ILogger<JobManager> logger, IJobStorage jobStorage)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _jobStorage = jobStorage;
        }

        /// <summary>
        /// Read access Jobs
        /// </summary>
        public ReadOnlyDictionary<string, Type> Jobs => new ReadOnlyDictionary<string, Type>(_jobStorage.Jobs);

        /// <summary>
        /// Read access Schedules
        /// </summary>
        public ReadOnlyDictionary<string, IScheduleProvider> Schedules =>
            new ReadOnlyDictionary<string, IScheduleProvider>(_jobStorage.Schedules);

        /// <summary>
        /// Adds a job.
        /// Job and Schedule are re-instantiated by DI every time needed.
        /// </summary>
        /// <typeparam name="TJob">Job Type</typeparam>
        /// <typeparam name="TSchedule">Schedule Type</typeparam>
        public void AddJob<TJob, TSchedule>() where TJob : IJob where TSchedule : ISchedule
        {
            AddJob<TJob>(new TypeScheduleProvider(typeof(TSchedule), _serviceProvider));
        }

        /// <summary>
        /// Adds a job.
        /// Job is instantiated via DI. Schedule instanced is re-used for each request.
        /// </summary>
        /// <param name="schedule">schedule</param>
        /// <typeparam name="TJob">Job Type</typeparam>
        public void AddJob<TJob>([NotNull] ISchedule schedule) where TJob : IJob
        {
            AddJob<TJob>(new InstanceScheduleProvider(schedule));
        }

        /// <summary>
        /// Adds a job with a ScheduleProvider.
        /// </summary>
        /// <param name="scheduleProvider">provider/factory for schedule</param>
        /// <typeparam name="TJob">job type</typeparam>
        public void AddJob<TJob>([NotNull] IScheduleProvider scheduleProvider) where TJob : IJob
        {
            AddOrUpdateJobInternal<TJob>(scheduleProvider, false, true);
            _logger.LogInformation("Job {jobKey} was added", typeof(TJob));
        }


        /// <summary>
        /// Updates schedule of the job.
        /// Job and Schedule are re-instantiated by DI every time needed.
        /// </summary>
        /// <typeparam name="TJob">Job Type</typeparam>
        /// <typeparam name="TSchedule">new Schedule Type</typeparam>
        public void UpdateSchedule<TJob, TSchedule>() where TJob : IJob where TSchedule : ISchedule
        {
            UpdateSchedule<TJob>(new TypeScheduleProvider(typeof(TSchedule), _serviceProvider));
        }

        /// <summary>
        /// Updates schedule of the job.
        /// Job is instantiated via DI. Schedule instanced is re-used for each request.
        /// </summary>
        /// <param name="schedule">new schedule</param>
        /// <typeparam name="TJob">Job Type</typeparam>
        public void UpdateSchedule<TJob>([NotNull] ISchedule schedule) where TJob : IJob
        {
            UpdateSchedule<TJob>(new InstanceScheduleProvider(schedule));
        }

        /// <summary>
        /// Updates schedule of the job.
        /// </summary>
        /// <param name="scheduleProvider">new ScheduleProvider</param>
        /// <typeparam name="TJob">Job Type</typeparam>
        public void UpdateSchedule<TJob>([NotNull] IScheduleProvider scheduleProvider) where TJob : IJob
        {
            AddOrUpdateJobInternal<TJob>(scheduleProvider, true, false);
            _logger.LogInformation("Schedule for job {jobKey} was updated", typeof(TJob));
        }

        /// <summary>
        /// Adds or updates the job.
        /// </summary>
        /// <param name="scheduleProvider">ScheduleProvider</param>
        /// <typeparam name="TJob">Job Type</typeparam>
        public void AddOrUpdate<TJob>([NotNull] IScheduleProvider scheduleProvider) where TJob : IJob
        {
            AddOrUpdateJobInternal<TJob>(scheduleProvider, true, true);
        }
        
        private void AddOrUpdateJobInternal<TJob>(IScheduleProvider scheduleProvider, bool update, bool add)
            where TJob : IJob
        {
            _jobStorage.AddOrUpdateJobInternal<TJob>(scheduleProvider, update, add);
        }

        /// <summary>
        /// Removes the job.
        /// If job is running, current run of this job will be finished (no cancellation requested).
        /// </summary>
        public void RemoveJob<TJob>() where TJob : IJob
        {
            RemoveJob(typeof(TJob).FullName);
        }

        /// <summary>
        /// Removes the job.
        /// If job is running, current run of this job will be finished (no cancellation requested).
        /// </summary>
        public void RemoveJob(string jobKey)
        {
            _logger.LogInformation("Job {jobKey} is removed from Scheduler", jobKey);
            _jobStorage.Remove(jobKey);
        }
    }
}