using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using AsyncScheduler.History;
using AsyncScheduler.JobStorage;
using AsyncScheduler.Restrictions;
using AsyncScheduler.Schedules;
using AsyncSchedulerTest.TestData;
using AsyncSchedulerTest.TestUtils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AsyncSchedulerTest
{
    public class SchedulerMultipleJobsTest
    {
        private readonly Scheduler _scheduler;
        private readonly SimpleJob _simpleJobInstance;

        public SchedulerMultipleJobsTest(ITestOutputHelper testOutputHelper)
        {
            _simpleJobInstance = new SimpleJob();
            var serviceProvider = new TestActivator(_simpleJobInstance);
            var jobManager = new JobManager(serviceProvider, new XUnitLogger<JobManager>(testOutputHelper),
                new InMemoryStorage());
            _scheduler = new Scheduler(serviceProvider, new XUnitLogger<Scheduler>(testOutputHelper),
                new UtcSchedulerClock(), jobManager);
        }

        [Fact]
        public async Task AddMoreJobs_ScheduleOnce_ExecutesAllJobs()
        {
            _scheduler.JobManager.AddJob<SimpleJob, ScheduleOnce>();
            _scheduler.JobManager.AddJob<NotImplementedJob, ScheduleOnce>();
            _scheduler.JobManager.AddJob<AsyncExceptionJob, ScheduleOnce>();
            await RunScheduler(TimeSpan.FromSeconds(1));
            var jobKey = typeof(SimpleJob).FullName;
            var lastSuccessfulJobResult = _scheduler.JobHistory.GetLastSuccessfulJobResult(jobKey);
            lastSuccessfulJobResult.Should().NotBeNull();

            _scheduler.JobHistory.GetLastSuccessfulJobResult(typeof(NotImplementedJob).FullName).Should().BeNull();
            _scheduler.JobHistory.GetLastSuccessfulJobResult(typeof(AsyncExceptionJob).FullName).Should().BeNull();

            _scheduler.JobHistory.GetLastJobResult(typeof(NotImplementedJob).FullName).Should().NotBeNull();
            _scheduler.JobHistory.GetLastJobResult(typeof(AsyncExceptionJob).FullName).Should().NotBeNull();
        }

        [Fact]
        public async Task AddMoreJobs_RestrictParallelExecution_ExecutesOnlyOneJobs()
        {
            _scheduler.LoopDelay = TimeSpan.FromSeconds(3);
            _scheduler.AddRestriction(new ConcurrentJobRestriction
            {
                MaximumParallelJobs = 1
            });
            _scheduler.JobManager.AddJob<SimpleJob, ScheduleOnce>();
            _scheduler.JobManager.AddJob<AsyncExceptionJob, ScheduleOnce>();
            await RunScheduler(TimeSpan.FromSeconds(1));

            // Only one job should have been executed:
            var sj = _scheduler.JobHistory.GetLastJobResult(typeof(SimpleJob).FullName);
            var aej = _scheduler.JobHistory.GetLastJobResult(typeof(AsyncExceptionJob).FullName);

            var jobHistoryEntries = new[] {sj, aej};
            jobHistoryEntries.Should().ContainSingle(e => e != null);
            jobHistoryEntries.Where(e => e == null).Should().HaveCount(1);
        }


        private async Task RunScheduler(TimeSpan schedulerTime)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var schedulerTask = _scheduler.Start(cancellationTokenSource.Token);
            // ReSharper disable MethodSupportsCancellation
            await Task.Delay(schedulerTime).ContinueWith((t) => cancellationTokenSource.Cancel());
            var schedulerFinishTimeout = TimeSpan.FromSeconds(1);
            await Task.WhenAny(schedulerTask, Task.Delay(schedulerFinishTimeout));
            // ReSharper restore MethodSupportsCancellation
        }
    }
}