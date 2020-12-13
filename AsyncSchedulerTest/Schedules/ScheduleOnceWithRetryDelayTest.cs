using System;
using AsyncScheduler.History;
using AsyncScheduler.Schedules;
using FluentAssertions;
using Xunit;

namespace AsyncSchedulerTest.Schedules
{
    public class ScheduleOnceWithRetryDelayTest
    {
        private readonly ScheduleOnceWithRetryDelay _schedule = new ScheduleOnceWithRetryDelay();
        
        private readonly string _jobKey = "keyNotUsed";

        [Fact]
        public void ShouldRunImmediately()
        {
            _schedule.GetExecutionPriority(_jobKey, null, null,
                DateTime.UtcNow).Should().BeGreaterThan(0);
        }
        
        [Fact]
        public void ShouldOnlyRunOnceSuccessful()
        {
            var success = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2)), _jobKey, JobResult.Success);
            _schedule.GetExecutionPriority(_jobKey, success, success,
                DateTime.UtcNow).Should().Be(0);
        }
        
        [Fact]
        public void ShouldRetry()
        {
            var failure = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)), _jobKey, JobResult.Failure);
            _schedule.GetExecutionPriority(_jobKey, failure, null,
                DateTime.UtcNow).Should().Be(1);
        }
        
        [Fact]
        public void ShouldNotRetryInsideOfRetryDelay()
        {
            _schedule.RetryDelay = TimeSpan.FromMinutes(5);
            var failure = new JobHistoryEntry(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2)), _jobKey, JobResult.Failure);
            _schedule.GetExecutionPriority(_jobKey, failure, null,
                DateTime.UtcNow).Should().Be(0);
        }
    }
}