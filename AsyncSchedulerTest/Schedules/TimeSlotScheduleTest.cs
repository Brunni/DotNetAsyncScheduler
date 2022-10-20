using System;
using AsyncScheduler.History;
using AsyncScheduler.Schedules;
using FluentAssertions;
using Xunit;

namespace AsyncSchedulerTest.Schedules
{
    public class TimeSlotScheduleTest
    {
        private readonly TimeSlotSchedule _timeSlotSchedule = new()
        {
            StartTime = StartTime,
            SlotTime = TimeSpan.FromMinutes(10)
        };

        private static readonly DateTime StartTime = new(2020, 8, 2, 20, 00, 00);
        private readonly string _jobKey = "keyNotUsed";

        [Fact]
        public void ShouldOnlyStartAfterTime()
        {
            _timeSlotSchedule.GetExecutionPriority(_jobKey, null, null,
                StartTime.Subtract(TimeSpan.FromSeconds(2))).Should().Be(0);
            
            _timeSlotSchedule.GetExecutionPriority(_jobKey, null, null,
                StartTime.AddSeconds(2)).Should().BeGreaterThan(0);
        }
        
        [Fact]
        public void ShouldNotStartAfterTimeSlot()
        {
            _timeSlotSchedule.GetExecutionPriority(_jobKey, null, null,
                StartTime.AddMinutes(10).AddSeconds(1)).Should().Be(0);
        }
        
        [Fact]
        public void ShouldOnlyRunOnceInTimeSlot()
        {
            var successInTimeSlot = new JobHistoryEntry(StartTime.AddMinutes(2), _jobKey, JobResult.Success);
            _timeSlotSchedule.GetExecutionPriority(_jobKey, successInTimeSlot, successInTimeSlot,
                StartTime.Add(TimeSpan.FromSeconds(1))).Should().Be(0);
        }
        
        [Fact]
        public void ShouldRerunImmediately_IfFailedInTimeSlot()
        {
            var successInTimeSlot = new JobHistoryEntry(StartTime.AddMinutes(2), _jobKey, JobResult.Failure);
            _timeSlotSchedule.GetExecutionPriority(_jobKey, successInTimeSlot, null,
                StartTime.Add(TimeSpan.FromSeconds(1))).Should().BeGreaterThan(0);
        }
    }
}