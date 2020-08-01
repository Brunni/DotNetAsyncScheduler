using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler.History;
using AsyncScheduler.Restrictions;
using AsyncScheduler.Schedules;
using AsyncScheduler.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace AsyncScheduler
{
    public class Scheduler
    {
        

        private readonly ConcurrentDictionary<string, Task> _runningJobs = new ConcurrentDictionary<string, Task>();

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Scheduler> _logger;
        private readonly ISchedulerClock _clock;
        private readonly List<IJobStartRestriction> _jobRestrictions = new List<IJobStartRestriction>();
        /// <summary>
        /// Determines the delay between each run, where the schedules are checked.
        /// </summary>
        private readonly TimeSpan _heartBeatDelay;

        public Scheduler(IServiceProvider serviceProvider, ILogger<Scheduler> logger, ISchedulerClock clock, JobManager jobManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _clock = clock;
            JobManager = jobManager;
            _heartBeatDelay = TimeSpan.FromSeconds(5); //TODO: Make public
        }

        public JobManager JobManager { get; }

        public IJobHistory JobHistory { get; } = new JobHistory();

        public void AddRestriction(IJobStartRestriction jobStartRestriction)
        {
            _jobRestrictions.Add(jobStartRestriction);
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Scheduler...");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogTrace("Checking for tasks ...");

                    //TODO: Change logic to only call scheduler once.
                    var jobQueue = JobManager.Jobs.Where(
                            j => !IsJobRunning(j.Key) && IsJobScheduled(j.Key))
                        .OrderByDescending(pair => GetJobExecutionPriority(pair.Key));
                    
                    foreach (var job in jobQueue)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        CheckRestrictionsAndStartJob(cancellationToken, job);
                    }

                    await Task.Delay(_heartBeatDelay, cancellationToken);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.LogDebug(e, "Execution loop was cancelled by CancellationToken");
            }

            Task finishUpTask = null;
            try
            {
                _logger.LogInformation("Scheduler stopped. Waiting for running jobs to finish: {jobCount}", _runningJobs.Keys);
                // Await all running tasks
                finishUpTask = Task.WhenAll(_runningJobs.Select(pair => pair.Value).ToList());
                await finishUpTask;
            }
            catch (OperationCanceledException e)
            {
                _logger.LogTrace(e, "A Job was cancelled by OperationCanceledException");
                if (finishUpTask?.Exception?.InnerExceptions != null && finishUpTask.Exception.InnerExceptions.Any())
                {
                    foreach (var innerEx in finishUpTask.Exception.InnerExceptions)
                    {
                        _logger.LogTrace(innerEx, "Job was cancelled");
                    }
                }
            }
        }

        private void CheckRestrictionsAndStartJob(CancellationToken cancellationToken, KeyValuePair<string, Type> job)
        {
            using (_logger.BeginScope("job", job.Key))
            {
                // Restriction check depends on already started jobs and therefore needs to be executed here.
                if (IsStartRestricted(job.Key))
                {
                    _logger.LogDebug("Job {jobKey} not started because of restrictions", job.Key);
                    return;
                }

                StartJob(cancellationToken, job);
            }
        }

        private bool IsJobScheduled(string jobKey)
        {
            var executionPriority = GetJobExecutionPriority(jobKey);
            var b = executionPriority > 0;
            return b;
        }

        private int GetJobExecutionPriority(string jobKey)
        {
            var schedule = GetSchedule(jobKey);
            if (schedule == null) return 0;
            
            var jobHistoryEntry = JobHistory.GetLastJobResult(jobKey);
            var lastSuccessfulExecution = JobHistory.GetLastSuccessfulJobResult(jobKey);
            var executionPriority =
                schedule.GetExecutionPriority(jobKey, jobHistoryEntry, lastSuccessfulExecution, _clock.GetNow());
            _logger.LogTrace("Execution priority from scheduler for job {jobKey} is {priority}", jobKey, executionPriority);
            return executionPriority;

        }

        private ISchedule GetSchedule(string jobKey)
        {
            JobManager.Schedules.TryGetValue(jobKey, out var scheduleProvider);
            if (scheduleProvider == null)
            {
                _logger.LogTrace("ScheduleProvider for job {jobKey} already removed. Skipping job", jobKey);
                return null;
            }

            var schedule = scheduleProvider.GetSchedule();
            if (schedule == null)
            {
                _logger.LogWarning("Schedule for job {jobKey} not available", jobKey);
                return null;
            }

            return schedule;
        }

        private bool IsJobRunning(string jobKey)
        {
            return _runningJobs.ContainsKey(jobKey) && _runningJobs[jobKey] != null;
        }

        private bool IsStartRestricted(string jobKey)
        {
            var runningJobs = _runningJobs.Where(j => j.Value != null).Select(j => j.Key);
            return _jobRestrictions.Select(restriction =>
                {
                    var restrictStart = restriction.RestrictStart(jobKey, runningJobs);
                    if (restrictStart)
                    {
                        _logger.LogTrace("JobStartRestriction for {jobKey} detected by {restriction}", jobKey,
                            restriction.GetType());
                    }

                    return restrictStart;
                })
                .Aggregate(false, (b0, b1) => b0 || b1);
        }

        private void StartJob(CancellationToken cancellationToken, KeyValuePair<string, Type> job)
        {
            _logger.LogInformation("Job {jobKey} is triggered", job.Key);
            var jobInstance = CreateJobInstance(job);
            if (jobInstance == null)
            {
                _logger.LogWarning("Cannot create service! Skipping job {jobKey}", job.Key);
                return;
            }

            _logger.LogTrace("Job {jobKey} is starting", job.Key);
            try
            {
                var jobTask = jobInstance.Start(cancellationToken);
                // We don't pass cancellation token to ContinueWith to allow record of result
                var jobRun = jobTask
                    // ReSharper disable once MethodSupportsCancellation
                    .ContinueWith(task => ResolveTaskEnd(job.Key, task));

                _runningJobs[job.Key] = jobRun;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error starting job {jobKey}", job.Key);
            }
        }

        [MustUseReturnValue]
        [CanBeNull]
        private IJob CreateJobInstance(KeyValuePair<string, Type> job)
        {
            try
            {
                var service = (IJob) _serviceProvider.GetService(job.Value);
                return service;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to create instance of {jobClass}", job.Value);
                return null;
            }
        }

        private async void ResolveTaskEnd(string jobKey, Task<object> task)
        {
            try
            {
                var result = await task;
                _logger.LogInformation("Job {jobKey} finished: {result}", jobKey, result);
                JobHistory.Add(new JobHistoryEntry
                {
                    ExecutionTime = _clock.GetNow(),
                    JobKey = jobKey,
                    Result = result,
                    JobResult = JobResult.Success
                });
            }
            catch (OperationCanceledException e)
            {
                _logger.LogInformation(e, "Job {jobKey} was cancelled", jobKey);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during execution of job {jobKey}", jobKey);
                JobHistory.Add(new JobHistoryEntry
                {
                    ExecutionTime = _clock.GetNow(),
                    JobKey = jobKey,
                    JobResult = JobResult.Failure
                });
            }
            finally
            {
                _runningJobs.TryRemove(jobKey, out _);
                if (task is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}