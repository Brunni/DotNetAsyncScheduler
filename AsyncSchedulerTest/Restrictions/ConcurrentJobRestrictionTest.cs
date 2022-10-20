using AsyncScheduler.Restrictions;
using FluentAssertions;
using Xunit;

namespace AsyncSchedulerTest.Restrictions
{
    public class ConcurrentJobRestrictionTest
    {
        [Fact]
        public void ShouldNotRestrictJobsWhenLimitNotReached()
        {
            var concurrentJobRestriction = new ConcurrentJobRestriction {MaximumParallelJobs = 3};

            concurrentJobRestriction.RestrictStart("job", new[] {"1", "2"}).Should().BeFalse();
        }

        [Fact]
        public void ShouldRestrictJobsWhenLimitReached()
        {
            var concurrentJobRestriction = new ConcurrentJobRestriction {MaximumParallelJobs = 3};

            concurrentJobRestriction.RestrictStart("job", new[] {"1", "2", "3"}).Should().BeTrue();
        }

        [Fact]
        public void ShouldNotRestrictJobsInExceptionList()
        {
            var concurrentJobRestriction = new ConcurrentJobRestriction {MaximumParallelJobs = 1, ExceptionList = new[] {"job"}};

            concurrentJobRestriction.RestrictStart("job", new[] {"1", "2"}).Should().BeFalse();
        }

        [Fact]
        public void ShouldNotCountOneJobInExceptionList()
        {
            var concurrentJobRestriction = new ConcurrentJobRestriction {MaximumParallelJobs = 2, ExceptionList = new[] {"job"}};

            concurrentJobRestriction.RestrictStart("1", new[] {"job", "2"}).Should().BeFalse();
        }

        [Fact]
        public void ShouldNotCountAllJobsInExceptionList()
        {
            var concurrentJobRestriction = new ConcurrentJobRestriction {MaximumParallelJobs = 1, ExceptionList = new[] {"job2", "job"}};

            concurrentJobRestriction.RestrictStart("1", new[] {"job", "job2"}).Should().BeFalse();
        }
    }
}