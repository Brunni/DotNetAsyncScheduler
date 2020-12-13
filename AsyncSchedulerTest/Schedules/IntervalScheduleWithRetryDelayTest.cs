using System;
using AsyncScheduler.History;
using AsyncScheduler.Schedules;
using FluentAssertions;
using Xunit;

namespace AsyncSchedulerTest.Schedules
{
    public class IntervalScheduleWithRetryDelayTest
    {
        private readonly IntervalScheduleWithRetryDelay _schedule = new IntervalScheduleWithRetryDelay(TimeSpan.FromMinutes(2));

        private readonly string _jobKey = "keyNotUsed";

        [Fact]
        public void ShouldRunImmediately()
        {
            _schedule.GetExecutionPriority(_jobKey, null, null,
                DateTime.UtcNow).Should().BeGreaterThan(0);
        }
        
        [Fact]
        public void ShouldNotRerunImmediatelyOnSuccess()
        {
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)), _jobKey, JobResult.Success);
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().Be(0);
        }
        
        [Fact]
        public void ShouldNotRerunImmediatelyOnFailure()
        {
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(20)), _jobKey, JobResult.Failure);
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().Be(0);
        }
        
        [Fact]
        public void ShouldRerunOnFailure()
        {
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)), _jobKey, JobResult.Failure);
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().Be(1);
        }
        
        [Fact]
        public void ShouldUseCustomRerunDelayAndNotRerunEarly()
        {
            _schedule.RetryDelay = TimeSpan.FromMinutes(1);
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(0.75)), _jobKey, JobResult.Failure);
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().Be(0);
        }
        
        [Fact]
        public void ShouldUseCustomRerunDelayAndCustomPriority()
        {
            _schedule.RetryDelay = TimeSpan.FromMinutes(1);
            _schedule.Priority = 4;
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1.2)), _jobKey, JobResult.Failure);
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().Be(4);
        }
        
        
        [Fact]
        public void ShouldBeReTriggeredWhenTimesUp()
        {
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2.01)), _jobKey, JobResult.Success);
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().BeGreaterThan(0);
        }
        
        [Fact]
        public void CustomPriorityShouldBeUsed()
        {
            _schedule.Priority = 5;
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2.1)), _jobKey, JobResult.Success);
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().Be(5);
        }

        [Fact]
        public void PriorityShouldIncreaseByMinutesDelay()
        {
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(8.5)), _jobKey, JobResult.Success);
            // Priority is the delay in minutes from when it should have actually been triggered
            var increasedPriority = 6;
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().Be(increasedPriority);
        }
        
        [Fact]
        public void CustomPriorityShouldBeUsedWhenDelayed()
        {
            _schedule.Priority = 2;
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(8.5)), _jobKey, JobResult.Success);
            // Priority is the delay in minutes from when it should have actually been triggered
            var increasedPriority = 12;
            _schedule.GetExecutionPriority(_jobKey, success, success, DateTime.UtcNow)
                .Should().Be(increasedPriority);
        }
    }
}