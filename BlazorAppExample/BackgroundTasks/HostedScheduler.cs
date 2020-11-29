using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using AsyncScheduler.Schedules;
using Microsoft.Extensions.Hosting;

namespace BlazorAppExample.BackgroundTasks
{
    public class HostedScheduler : IHostedService
    {
        private CancellationTokenSource _cancellationTokenSource;

        private readonly Scheduler _scheduler;
        private Task _schedulerTask;

        public HostedScheduler(Scheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _scheduler.JobManager.AddJob<EndlessLoopTask, ScheduleOnce>();
            _scheduler.JobManager.AddJob<SimpleTask>(new IntervalSchedule(TimeSpan.FromSeconds(20)));
            _scheduler.JobManager.AddJob<SimpleTask2, ScheduleNever>();

            _schedulerTask = _scheduler.Start(_cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_schedulerTask != null)
            {
                _cancellationTokenSource?.Cancel();
                await _schedulerTask;
            }
        }
    }
}