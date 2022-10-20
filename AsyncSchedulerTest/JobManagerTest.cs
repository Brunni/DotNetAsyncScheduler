using System;
using AsyncScheduler;
using AsyncScheduler.JobStorage;
using AsyncSchedulerTest.TestData;
using AsyncSchedulerTest.TestUtils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AsyncSchedulerTest
{
    public class JobManagerTest
    {
        private readonly JobManager _jobManager;

        public JobManagerTest(ITestOutputHelper testOutputHelper)
        {
            var logger = new XUnitLogger<JobManager>(testOutputHelper);
            _jobManager = new JobManager(new TestActivator(), logger, new InMemoryStorage());
        }

        [Fact]
        public void AddJobViaWithScheduleInstance_JobAdded()
        {
            _jobManager.AddJob<NotImplementedJob>(new NotImplementedSchedule("1"));
            _jobManager.Jobs.Should().HaveCount(1);
            _jobManager.Schedules.Should().HaveCount(1);

            string? jobKey = typeof(NotImplementedJob).FullName;
            _jobManager.Jobs[jobKey!].Should().Be<NotImplementedJob>();
            // ReSharper disable once SuspiciousTypeConversion.Global
            ((NotImplementedSchedule?) _jobManager.Schedules[jobKey!].GetSchedule())?.Marker.Should().Be("1");
        }
        
        [Fact]
        public void AddJobViaGeneric_JobAdded()
        {
            _jobManager.AddJob<NotImplementedJob, NotImplementedSchedule>();
            _jobManager.Jobs.Should().HaveCount(1);
            _jobManager.Schedules.Should().HaveCount(1);

            string? jobKey = typeof(NotImplementedJob).FullName;
            _jobManager.Jobs[jobKey!].Should().Be<NotImplementedJob>();
            var firstSchedule = (NotImplementedSchedule?) _jobManager.Schedules[jobKey!].GetSchedule();
            var secondSchedule = (NotImplementedSchedule?) _jobManager.Schedules[jobKey!].GetSchedule();
            firstSchedule?.Marker.Should().Be("DI");
            // Each instance is newly requested.
            firstSchedule.Should().NotBeSameAs(secondSchedule);
        }
        
        [Fact]
        public void AddJobTwice_Throws()
        {
            _jobManager.AddJob<NotImplementedJob>(new NotImplementedSchedule("1"));
            Action add = () => _jobManager.AddJob<NotImplementedJob>(new NotImplementedSchedule("2"));
            add.Should().Throw<Exception>();
            
            string? jobKey = typeof(NotImplementedJob).FullName;
            ((NotImplementedSchedule?) _jobManager.Schedules[jobKey!].GetSchedule())?.Marker.Should().Be("1");
        }
        
        [Fact]
        public void UpdateJobWithScheduleInstance_JobUpdated()
        {
            _jobManager.AddJob<NotImplementedJob>(new NotImplementedSchedule("1"));
            _jobManager.UpdateSchedule<NotImplementedJob>(new NotImplementedSchedule("2"));
            _jobManager.Jobs.Should().HaveCount(1);
            _jobManager.Schedules.Should().HaveCount(1);

            string? jobKey = typeof(NotImplementedJob).FullName;
            _jobManager.Jobs[jobKey!].Should().Be<NotImplementedJob>();
            ((NotImplementedSchedule?) _jobManager.Schedules[jobKey!].GetSchedule())?.Marker.Should().Be("2");
        }
        
        [Fact]
        public void AddTwoJobs_RemoveOneJob()
        {
            _jobManager.AddJob<NotImplementedJob>(new NotImplementedSchedule("1"));
            _jobManager.AddJob<SimpleJob>(new NotImplementedSchedule("J2"));
            _jobManager.Jobs.Should().HaveCount(2);
            _jobManager.Schedules.Should().HaveCount(2);

            _jobManager.RemoveJob<SimpleJob>().Should().BeTrue();
            
            _jobManager.Jobs.Should().HaveCount(1);
            _jobManager.Schedules.Should().HaveCount(1);
        }
        
        [Fact]
        public void NoJobs_RemoveOneJob_NotRemove()
        {
            _jobManager.RemoveJob<SimpleJob>().Should().BeFalse();
        }
    }
}