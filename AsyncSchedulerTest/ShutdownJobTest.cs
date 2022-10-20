using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using AsyncScheduler.JobStorage;
using AsyncScheduler.Schedules;
using AsyncSchedulerTest.TestData;
using AsyncSchedulerTest.TestUtils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AsyncSchedulerTest
{
    public class ShutdownJobTest
    {
        private readonly Scheduler _scheduler;
        private readonly ShutdownJob _shutdownJobInstancee;

        public ShutdownJobTest(ITestOutputHelper testOutputHelper)
        {
            _shutdownJobInstancee = new ShutdownJob();
            var serviceProvider = new TestActivator(null, _shutdownJobInstancee);
            var jobManager = new JobManager(serviceProvider, new XUnitLogger<JobManager>(testOutputHelper),
                new InMemoryStorage());
            _scheduler = new Scheduler(serviceProvider, new XUnitLogger<Scheduler>(testOutputHelper),
                new UtcSchedulerClock(), jobManager);
        }

        [Fact]
        public async Task TestShutdown()
        {
            _scheduler.JobManager.AddJob<ShutdownJob, ScheduleNever>();
            await RunScheduler(TimeSpan.FromSeconds(1));
            _shutdownJobInstancee.ShutdownCount.Should().Be(1);
        }

        private async Task RunScheduler(TimeSpan schedulerTime)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var schedulerTask = _scheduler.Start(cancellationTokenSource.Token);
            // ReSharper disable MethodSupportsCancellation
            await Task.Delay(schedulerTime).ContinueWith(_ => cancellationTokenSource.Cancel());
            var schedulerFinishTimeout = TimeSpan.FromSeconds(1);
            await Task.WhenAny(schedulerTask, Task.Delay(schedulerFinishTimeout));
            cancellationTokenSource.Cancel();
            await schedulerTask;
        }
    }
}