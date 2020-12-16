using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler.History;
using AsyncScheduler.QuickStart;
using AsyncScheduler.Restrictions;
using AsyncScheduler.Schedules;
using AsyncScheduler.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace AsyncScheduler
{
    /// <summary>
    /// Central entry class for the AsyncScheduler.
    /// The Scheduler has methods to add/remove restrictions and to start the scheduler.
    /// As it is the central class it allows access to the JobManager to add/remove jobs and the JobHistory.
    /// </summary>
    /// <remarks>General behavior: A loop is executed every 5 seconds (<see cref="LoopDelay"/>) and checks all Schedules to see, which jobs should be executed.
    /// When jobs should be executed, they are ordered by their execution policy, then each is checked for restrictions and started.</remarks>
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
        public TimeSpan LoopDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Use DI framework to create the Scheduler.
        /// </summary>
        public Scheduler(IServiceProvider serviceProvider, ILogger<Scheduler> logger, ISchedulerClock clock,
            JobManager jobManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _clock = clock;
            JobManager = jobManager;
        }

        /// <summary>
        /// Access to the JobManager to add / remove jobs and their schedules.
        /// </summary>
        public JobManager JobManager { get; }

        /// <summary>
        /// History of job executions.
        /// </summary>
        public IJobHistory JobHistory { get; } = new JobHistory();

        /// <summary>
        /// List of currently active jobs
        /// </summary>
        public IReadOnlyCollection<string> CurrentlyRunningJobs => _runningJobs.Keys.ToList();

        /// <summary>
        /// Jobs added here are executed in the next loop (when no restrictions apply).
        /// Job is removed from queue, when executed or restrictions apply during execution.
        /// </summary>
        private ConcurrentQueue<QuickStartRequest> QuickStartQueue { get; } = new ConcurrentQueue<QuickStartRequest>();

        /// <summary>
        /// Jobs added here are executed in the next loop (when no restrictions apply).
        /// </summary>
        /// <returns>task which resolves when job is started to true, otherwise (e.g. restrictions) to false</returns>
        /// <remarks>Job needs to exist in Job list already.</remarks>
        /// <typeparam name="T">Job</typeparam>
        public Task<QuickStartResult> QuickStart<T>(CancellationToken cancellationToken = default) where T : IJob
        {
            return QuickStart(typeof(T).FullName, cancellationToken);
        }

        /// <summary>
        /// Jobs added here are executed in the next loop (when no restrictions apply).
        /// </summary>
        /// <returns>task which resolves when job is started to true, otherwise (e.g. restrictions) to false</returns>
        /// <remarks>Job needs to exist in Job list already.</remarks>
        public Task<QuickStartResult> QuickStart(string jobKey, CancellationToken cancellationToken = default)
        {
            var quickStartRequest = new QuickStartRequest(jobKey, cancellationToken);
            QuickStartQueue.Enqueue(quickStartRequest);
            return quickStartRequest.CompletionTask;
        }

        /// <summary>
        /// Add a job start restriction. The restriction prevents the starting of the job, although it is scheduled.
        /// </summary>
        /// <param name="jobStartRestriction">the restriction</param>
        public void AddRestriction(IJobStartRestriction jobStartRestriction)
        {
            _jobRestrictions.Add(jobStartRestriction);
        }

        /// <summary>
        /// Start the scheduling loop. Use the cancellationToken to stop it again.
        /// After stopping it, it can be restarted.
        /// </summary>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Task of the job scheduling loop</returns>
        public async Task Start(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Scheduler...");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogTrace("New Scheduler cycle: Running jobs: {runningJobs}. Checking for new jobs...", _runningJobs);

                    ExecuteQuickStarts(cancellationToken);

                    var priorityJobDictionary = JobManager.Jobs.Where(
                            j => !IsJobRunning(j.Key))
                        .Select(j => new KeyValuePair<string, int>(j.Key, GetJobExecutionPriority(j.Key)))
                        .Where(jp => jp.Value > 0)
                        .ToDictionary(jp => jp.Key, jp => jp.Value);

                    var jobQueue = JobManager.Jobs
                        .Where(pair => priorityJobDictionary.ContainsKey(pair.Key))
                        .OrderByDescending(pair => priorityJobDictionary[pair.Key]);

                    foreach (var job in jobQueue)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        CheckRestrictionsAndStartJob(cancellationToken, job);
                    }

                    await Task.Delay(LoopDelay, cancellationToken);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.LogDebug(e, "Execution loop was cancelled by CancellationToken");
            }

            Task finishUpTask = null;
            try
            {
                _logger.LogInformation("Scheduler stopped. Waiting for running jobs to finish: {jobCount}",
                    _runningJobs.Keys);
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

            await ExecuteShutdownMethods();
        }

        private async Task ExecuteShutdownMethods()
        {
            var shutdownTasksWithErrorHandler = JobManager.Jobs
                .Where(j => typeof(IShutdownJob).IsAssignableFrom(j.Value))
                .Select(pair => new Tuple<string, Task<string>>(pair.Key, ExecuteShutdownJob(pair)))
                .Select(t => t.Item2.ContinueWith(task => HandleShutdownFinished(t.Item1, task)));

            await Task.WhenAll(shutdownTasksWithErrorHandler);
        }

        private async Task HandleShutdownFinished(string jobKey, Task<string> shutdownTask)
        {
            try
            {
                var result = await shutdownTask;
                _logger.LogInformation("Shutdown job for {jobKey} finished with result: {result}", jobKey, result);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to shutdown job {jobKey}", jobKey);
            }
        }

        private async Task<string> ExecuteShutdownJob(KeyValuePair<string, Type> job)
        {
            using (_logger.BeginScope("job", job.Key))
            {
                var jobInstance = CreateJobInstance(job);
                if (jobInstance == null)
                {
                    _logger.LogWarning("Cannot create service! Skipping job {jobKey}", job.Key);
                    return "Job cannot be initialized";
                }

                try
                {
                    var shutdown = ((IShutdownJob) jobInstance).Shutdown();
                    return await shutdown;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Unable to shutdown job {jobKey}", job.Key);
                    return "Shutdown of job not executed";
                }
            }
        }

        private void ExecuteQuickStarts(CancellationToken cancellationToken)
        {
            while (QuickStartQueue.TryDequeue(out var quickStartRequest))
            {
                var resultJobKey = quickStartRequest.JobKey;
                if (resultJobKey == null)
                {
                    continue;
                }

                if (_runningJobs.ContainsKey(resultJobKey))
                {
                    quickStartRequest.MarkExecution(QuickStartResult.AlreadyRunning);
                    continue;
                }

                var job = JobManager.Jobs.Single(j => j.Key == resultJobKey);
                var success = CheckRestrictionsAndStartJob(cancellationToken, job);
                _logger.LogInformation("Quick start of {jobKey} was successful: {started}", job.Key, success);
                quickStartRequest.MarkExecution(success ? QuickStartResult.Started : QuickStartResult.Restricted);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private bool CheckRestrictionsAndStartJob(CancellationToken cancellationToken, KeyValuePair<string, Type> job)
        {
            using (_logger.BeginScope("job", job.Key))
            {
                // Restriction check depends on already started jobs and therefore needs to be executed here.
                if (IsStartRestricted(job.Key))
                {
                    _logger.LogDebug("Job {jobKey} not started because of restrictions", job.Key);
                    return false;
                }

                StartJob(cancellationToken, job);
                return true;
            }
        }

        private int GetJobExecutionPriority(string jobKey)
        {
            var schedule = GetSchedule(jobKey);
            if (schedule == null) return 0;

            var jobHistoryEntry = JobHistory.GetLastJobResult(jobKey);
            var lastSuccessfulExecution = JobHistory.GetLastSuccessfulJobResult(jobKey);
            try
            {
                var executionPriority = schedule.GetExecutionPriority(jobKey, jobHistoryEntry, lastSuccessfulExecution, _clock.GetNow());
                _logger.LogTrace("Execution priority from scheduler for job {jobKey} is {priority}", jobKey, executionPriority);
                return executionPriority;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to get the execution priority for job {jobKey} from schedule {scheduleType}. Will be retried in next cycle.", jobKey, schedule.GetType());
                return 0;
            }
        }

        private ISchedule GetSchedule(string jobKey)
        {
            JobManager.Schedules.TryGetValue(jobKey, out var scheduleProvider);
            if (scheduleProvider == null)
            {
                _logger.LogTrace("ScheduleProvider for job {jobKey} already removed. Skipping job", jobKey);
                return null;
            }

            try
            {
                var schedule = scheduleProvider.GetSchedule();
                if (schedule == null)
                {
                    _logger.LogWarning("Schedule for job {jobKey} not available", jobKey);
                    return null;
                }

                return schedule;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to get schedule for job {jobKey} from ScheduleProvider {ScheduleProviderType}", jobKey, scheduleProvider.GetType());
                return null;
            }
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
                        _logger.LogTrace("JobStartRestriction for {jobKey} detected by {restriction}", jobKey, restriction.GetType());
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
                JobHistory.Add(new JobHistoryEntry(_clock.GetNow(), job.Key, JobResult.Failure,
                    "JobStartFailed: " + e.Message));
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

        /// <summary>
        /// Method which is executed when a job finishes execution.
        /// </summary>
        private async void ResolveTaskEnd(string jobKey, Task<object> task)
        {
            try
            {
                var result = await task;
                _logger.LogInformation("Job {jobKey} finished: {result}", jobKey, result);
                JobHistory.Add(new JobHistoryEntry(_clock.GetNow(), jobKey, JobResult.Success, result));
            }
            catch (OperationCanceledException e)
            {
                _logger.LogInformation(e, "Job {jobKey} was cancelled", jobKey);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during execution of job {jobKey}", jobKey);
                JobHistory.Add(new JobHistoryEntry(_clock.GetNow(), jobKey, JobResult.Failure,
                    "JobFailed: " + e.Message));
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