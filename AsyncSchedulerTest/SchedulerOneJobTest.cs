using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using AsyncScheduler.History;
using AsyncScheduler.JobStorage;
using AsyncScheduler.Schedules;
using AsyncSchedulerTest.TestData;
using AsyncSchedulerTest.TestUtils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AsyncSchedulerTest
{
    public class SchedulerOneJobTest
    {
        private readonly Scheduler _scheduler;
        private readonly SimpleJob _simpleJobInstance;

        public SchedulerOneJobTest(ITestOutputHelper testOutputHelper)
        {
            _simpleJobInstance = new SimpleJob();
            var serviceProvider = new TestActivator(_simpleJobInstance);
            var jobManager = new JobManager(serviceProvider, new XUnitLogger<JobManager>(testOutputHelper),
                new InMemoryStorage());
            _scheduler = new Scheduler(serviceProvider, new XUnitLogger<Scheduler>(testOutputHelper),
                new UtcSchedulerClock(), jobManager);
        }

        [Fact]
        public async Task AddOneJob_ScheduleOnce_ExecutesJob()
        {
            _scheduler.JobManager.AddJob<SimpleJob, ScheduleOnce>();
            await RunScheduler(TimeSpan.FromSeconds(0.5));
            var jobKey = typeof(SimpleJob).FullName;
            var lastSuccessfulJobResult = _scheduler.JobHistory.GetLastSuccessfulJobResult(jobKey);
            lastSuccessfulJobResult.Should().NotBeNull();
            _scheduler.JobHistory.GetLastJobResult(jobKey).Should()
                .BeSameAs(lastSuccessfulJobResult);

            lastSuccessfulJobResult?.ExecutionTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            lastSuccessfulJobResult?.JobResult.Should().Be(JobResult.Success);
            lastSuccessfulJobResult?.JobKey.Should().Be(jobKey);
        }

        [Fact]
        public async Task AddOneJob_ScheduleNever_QuickStartJob_ExecutesJobViaQuickStart()
        {
            _scheduler.LoopDelay = TimeSpan.FromMilliseconds(100);
            _scheduler.JobManager.AddJob<SimpleJob, ScheduleNever>();
            var runSchedulerTask = RunScheduler(TimeSpan.FromSeconds(0.5));
            _scheduler.QuickStart<SimpleJob>();
            await runSchedulerTask;

            var jobKey = typeof(SimpleJob).FullName;
            var lastSuccessfulJobResult = _scheduler.JobHistory.GetLastSuccessfulJobResult(jobKey);
            lastSuccessfulJobResult.Should().NotBeNull();
            _scheduler.JobHistory.GetLastJobResult(jobKey).Should()
                .BeSameAs(lastSuccessfulJobResult);

            lastSuccessfulJobResult?.ExecutionTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            lastSuccessfulJobResult?.JobResult.Should().Be(JobResult.Success);
            lastSuccessfulJobResult?.JobKey.Should().Be(jobKey);
        }

        [Fact]
        public async Task AddStartFailingJob_ScheduleOnce_ExecutesJobAndMarksResult()
        {
            _scheduler.JobManager.AddJob<NotImplementedJob, ScheduleOnce>();
            await RunScheduler(TimeSpan.FromSeconds(0.5));
            var jobKey = typeof(NotImplementedJob).FullName;
            _scheduler.JobHistory.GetLastSuccessfulJobResult(jobKey).Should().BeNull();
            var lastResult = _scheduler.JobHistory.GetLastJobResult(jobKey);
            lastResult.Should().NotBeNull();

            lastResult?.ExecutionTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            lastResult?.JobResult.Should().Be(JobResult.Failure);
            lastResult?.JobKey.Should().Be(jobKey);
            lastResult?.ResultString.Should().StartWith("JobStartFailed");
        }

        [Fact]
        public async Task AddAsyncFailingJob_ScheduleOnce_ExecutesJobAndMarksResult()
        {
            _scheduler.JobManager.AddJob<AsyncExceptionJob, ScheduleOnce>();
            await RunScheduler(TimeSpan.FromSeconds(0.5));
            var jobKey = typeof(AsyncExceptionJob).FullName;
            _scheduler.JobHistory.GetLastSuccessfulJobResult(jobKey).Should().BeNull();
            var lastResult = _scheduler.JobHistory.GetLastJobResult(jobKey);
            lastResult.Should().NotBeNull();

            lastResult?.ExecutionTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            lastResult?.JobResult.Should().Be(JobResult.Failure);
            lastResult?.JobKey.Should().Be(jobKey);
            lastResult?.ResultString.Should().StartWith("JobFailed");
        }

        [Fact]
        public async Task AddOneJob_ScheduleEndless_ExecutesJobSeveralTimes()
        {
            var executionCountBefore = _simpleJobInstance.ExecutionCount;
            // Hint: We reduce the delay for each run
            _scheduler.LoopDelay = TimeSpan.FromSeconds(0.1);
            _scheduler.JobManager.AddJob<SimpleJob, ScheduleEndless>();

            // Act
            await RunScheduler(TimeSpan.FromSeconds(2));

            var jobKey = typeof(SimpleJob).FullName;
            var lastSuccessfulJobResult = _scheduler.JobHistory.GetLastSuccessfulJobResult(jobKey);
            lastSuccessfulJobResult.Should().NotBeNull();
            _scheduler.JobHistory.GetLastJobResult(jobKey).Should()
                .BeSameAs(lastSuccessfulJobResult);

            lastSuccessfulJobResult?.ExecutionTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            lastSuccessfulJobResult?.JobResult.Should().Be(JobResult.Success);
            lastSuccessfulJobResult?.JobKey.Should().Be(jobKey);

            (_simpleJobInstance.ExecutionCount - executionCountBefore).Should().BeInRange(6, 10);
        }

        [Fact]
        public async Task AddOneJob_ScheduleEndless_RemoveJobInBetween_ExecutesJobSeveralTimes()
        {
            var executionCountBefore = _simpleJobInstance.ExecutionCount;
            // Hint: We reduce the delay for each run
            _scheduler.LoopDelay = TimeSpan.FromSeconds(0.1);
            _scheduler.JobManager.AddJob<SimpleJob, ScheduleEndless>();

            // Act
            var schedulerTask = RunScheduler(TimeSpan.FromSeconds(2));
            // Remove job after 500 ms, so job is not triggered anymore
            await Task.Delay(500);
            _scheduler.JobManager.RemoveJob<SimpleJob>();
            await schedulerTask;

            var jobKey = typeof(SimpleJob).FullName;
            var lastSuccessfulJobResult = _scheduler.JobHistory.GetLastSuccessfulJobResult(jobKey);
            lastSuccessfulJobResult.Should().NotBeNull();
            _scheduler.JobHistory.GetLastJobResult(jobKey).Should()
                .BeSameAs(lastSuccessfulJobResult);

            lastSuccessfulJobResult?.ExecutionTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            lastSuccessfulJobResult?.JobResult.Should().Be(JobResult.Success);
            lastSuccessfulJobResult?.JobKey.Should().Be(jobKey);

            (_simpleJobInstance.ExecutionCount - executionCountBefore).Should().BeInRange(2, 5);
        }

        [Fact]
        public async Task AddOneJob_ScheduleOnce_UpdateScheduleTo_ScheduleEndless_ExecutesJobSeveralTimes()
        {
            var executionCountBefore = _simpleJobInstance.ExecutionCount;
            // Hint: We reduce the delay for each run
            _scheduler.LoopDelay = TimeSpan.FromSeconds(0.1);
            _scheduler.JobManager.AddJob<SimpleJob, ScheduleOnce>();

            // Act
            var schedulerTask = RunScheduler(TimeSpan.FromSeconds(3.5));
            // Task is only executed once then we update schedule and task is triggered again
            await Task.Delay(2000);
            (_simpleJobInstance.ExecutionCount - executionCountBefore).Should().Be(1, $" executionCountBefore was {executionCountBefore}");
            await Task.Delay(100);
            _scheduler.JobManager.UpdateSchedule<SimpleJob, ScheduleEndless>();
            await schedulerTask;

            var jobKey = typeof(SimpleJob).FullName;
            var lastSuccessfulJobResult = _scheduler.JobHistory.GetLastSuccessfulJobResult(jobKey);
            lastSuccessfulJobResult.Should().NotBeNull();
            _scheduler.JobHistory.GetLastJobResult(jobKey).Should()
                .BeSameAs(lastSuccessfulJobResult);

            lastSuccessfulJobResult?.ExecutionTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            lastSuccessfulJobResult?.JobResult.Should().Be(JobResult.Success);
            lastSuccessfulJobResult?.JobKey.Should().Be(jobKey);

            (_simpleJobInstance.ExecutionCount - executionCountBefore).Should().BeInRange(4, 15);
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