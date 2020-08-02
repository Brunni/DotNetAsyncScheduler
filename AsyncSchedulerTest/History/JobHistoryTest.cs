using System;
using AsyncScheduler.History;
using FluentAssertions;
using Xunit;

namespace AsyncSchedulerTest.History
{
    public class JobHistoryTest
    {
        private readonly JobHistory _jobHistory;
        private const string JobKey = "MyJob";

        public JobHistoryTest()
        {
            _jobHistory = new JobHistory();
        }

        [Fact]
        public void JobHistory_WithNoResults_ReturnsNothing()
        {
            _jobHistory.GetLastJobResult(JobKey).Should().BeNull();
            _jobHistory.GetLastSuccessfulJobResult(JobKey).Should().BeNull();
        }

        [Fact]
        public void JobHistory_WithOneResults_ReturnsNothingForOtherJobKeys()
        {
            AddJobHistoryEntry(JobResult.Success, TimeSpan.FromMinutes(2));
            _jobHistory.GetLastJobResult("OtherKey").Should().BeNull();
            _jobHistory.GetLastSuccessfulJobResult("OtherKey").Should().BeNull();
        }

        [Fact]
        public void JobHistory_WithFailingResult_ReturnsOnlyFailingResult()
        {
            var addedEntry = AddJobHistoryEntry(JobResult.Failure, TimeSpan.FromMinutes(2));
            var jobHistoryEntry = _jobHistory.GetLastJobResult(JobKey);
            jobHistoryEntry.Should().BeSameAs(addedEntry);

            _jobHistory.GetLastSuccessfulJobResult(JobKey).Should().BeNull();
        }

        [Fact]
        public void JobHistory_WithOnlySuccessResult_ReturnsOnlyFailingResult()
        {
            var addedEntry = AddJobHistoryEntry(JobResult.Success, TimeSpan.FromMinutes(2));
            _jobHistory.GetLastJobResult(JobKey).Should().BeSameAs(addedEntry);
            _jobHistory.GetLastSuccessfulJobResult(JobKey).Should().BeSameAs(addedEntry);
        }

        
        [Fact]
        public void JobHistory_WithOneFailureOneSuccessResult_ReturnsBothResult()
        {
            var addedEntry = AddJobHistoryEntry(JobResult.Success, TimeSpan.FromMinutes(2));
            var addedFailureEntry = AddJobHistoryEntry(JobResult.Failure, TimeSpan.FromMinutes(1));
            _jobHistory.GetLastJobResult(JobKey).Should().BeSameAs(addedFailureEntry);
            _jobHistory.GetLastSuccessfulJobResult(JobKey).Should().BeSameAs(addedEntry);
        }
        
        [Fact]
        public void JobHistory_WithMultipleSuccessResult_ReturnsOnlyLastAddedResult()
        {
            AddJobHistoryEntry(JobResult.Success, TimeSpan.FromMinutes(3));
            AddJobHistoryEntry(JobResult.Success, TimeSpan.FromMinutes(2));
            var addedEntry = AddJobHistoryEntry(JobResult.Success, TimeSpan.FromMinutes(1));
            _jobHistory.GetLastJobResult(JobKey).Should().BeSameAs(addedEntry);
            _jobHistory.GetLastSuccessfulJobResult(JobKey).Should().BeSameAs(addedEntry);
        }

        private JobHistoryEntry AddJobHistoryEntry(JobResult jobResult, TimeSpan timeBeforeNow)
        {
            var executionTime = DateTime.UtcNow.Subtract(timeBeforeNow);
            var jobHistoryEntry = new JobHistoryEntry(executionTime, JobKey, jobResult, "SomeString");
            _jobHistory.Add(jobHistoryEntry);
            return jobHistoryEntry;
        }
    }
}