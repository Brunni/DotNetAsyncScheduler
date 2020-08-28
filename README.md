# DotNetAsyncScheduler


Prerelease versions from Master
![CI-Release](https://github.com/Brunni/DotNetAsyncScheduler/workflows/CI-Release/badge.svg?branch=master)
available in [MyGet-Feed](https://www.myget.org/feed/dotnetasyncscheduler/package/nuget/DotnetAsyncScheduler)

## Requirements / Dependencies
* .NET Core 2.0 or .NET Framework 4.7.2

* Microsoft.Extensions.DependencyInjection.Abstractions
* Microsoft.Extensions.Logging.Abstractions
* JetBrains.Annotations

## Implementation Goals

* Simple triggering of async Jobs based on Schedules
* High integration with Dependency Injection for Jobs and Schedules via `Microsoft.Extensions.DependencyInjection.Abstractions`
* Split from Jobs and Schedules
* Allow implementing of restrictions (e.g. Mutex, Slow-Start)
* Use async-await and CancellationToken
* One job cannot run in parallel
* Intelligent error handling (e.g. allow immediate restart in Scheduler)
* Allow stopping / restarting and chaning of jobs/schedules on the fly
* Use logging via `Microsoft.Extensions.Logging.Abstractions`

What we don't have:
* Any kind of persistence of job runs or history (e.g. in Database)
* No serialization (yeay) of jobs or job parameters
* Jobs with parameters

## Execution model

In each cycle (cycle delay can be configured), the Scheduler pulls all Schedules of jobs, which are currently not running.
If a schedule returns a priority number greater than 0, the job is marked for execution.
The marked jobs are then ordered by the priority number. One after the other, the jobs are started, when the job restrictions allow this.

## Example

### Scheduler
```csharp
//setup your DI
var serviceProvider = new ServiceCollection()
    .RegisterAsyncScheduler()
    .BuildServiceProvider();

var scheduler = serviceProvider.GetService<Scheduler>();
if (scheduler == null)
{
    throw new NullReferenceException("Unable to get Scheduler");
}

var jobManager = scheduler.JobManager;

jobManager.AddJob<SimpleTask>(new TimeSlotSchedule
{
    StartTime = DateTime.UtcNow + TimeSpan.FromSeconds(20)
});
jobManager.AddJob<ExampleTask2>(new IntervalSchedule(TimeSpan.FromSeconds(25)));
jobManager.AddJob<DownloadTask>(new IntervalSchedule(TimeSpan.FromSeconds(10)));
jobManager.AddJob<EndlessLoopTask, ScheduleOnce>();
scheduler.AddRestriction(new ConcurrentJobRestriction
{
    MaximumParallelJobs = 10
});
scheduler.AddRestriction(new MutexRestriction(typeof(SimpleTask), typeof(DownloadTask)));

var schedulerTask = scheduler.Start(_cancellationTokenSource.Token);
await schedulerTask;
```

Hint: There is no stop method. Use the cancellationToken to stop the scheduling.

### Job

```csharp
public class DownloadTask : IJob
{
    private readonly ILogger<DownloadTask> _logger;

    public DownloadTask(ILogger<DownloadTask> logger)
    {
        _logger = logger;
    }

    public async Task<object> Start(CancellationToken cancellationToken)
    {
        var httpClient = new HttpClient();
        _logger.LogInformation("Starting download ...");
        await httpClient.GetAsync("https://www.google.com", cancellationToken);
        _logger.LogInformation("Download finished ...");
        return "Download success";
    }
}
```

## IJob

A job should highly use async methods to avoid blocking other jobs. If the job's main action uses not-async CPU intensive work, consider to spawn a new task/thread via `Task.Run` inside your Job.

The job may end with an exception in case of errors. The return value is logged using ToString().

## Schedules

Schedules define, when a Job is executed: If schedule returns int>0, the Job is started (when no restrictions apply). Jobs with higher numbers are started first, which is only relevant, when Restrictions (e.g. Mutex, MaxJobNumber) are applied.
A Schedule might handle retry in case of failures.

As Schedules can be created via DI, they may connect to a database / config file to read current configuration state.
In case of IO-Operations in Schedule consider increasing the `LoopDelay`.

### Predefined Schedules

There are several examples of schedule implementation available:

* ScheduleOnce: Schedule immediately, when not executed or failed
* ScheduleOnceWithRetryDelay: Schedule immediately. Retry on failure with certain configurable delay
* ScheduleEndless: Always re-schedule, when finished
* IntervalSchedule: Schedule based on a `TimeSpan` interval
* TimeSlotSchedule: Schedule based on a `Datetime` after which it should start

## Restrictions

Restrictions can be used to prevent Jobs from being started based on the current context (other running jobs). They are executed shortly before a Job is started.

Feel free to implement your own restrictions.

### Predefined Restrictions

* ConcurrentJobRestriction: Restrict number of parallel jobs
* MutexRestriction: Restrict certain jobs from running in parallel
* SlowStartRestriction: Don't start all jobs at once (experimentell)

## Disclaimer

Use at your own risk.

## Alternatives

* Hangfire
* Quartz.NET
* Fluentscheduler

