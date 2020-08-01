using System;
using System.Collections.Generic;
using System.Linq;
using AsyncScheduler.History;

namespace AsyncScheduler.Restrictions
{
    /// <summary>
    /// Example implementation for slow starting of jobs.
    /// Idea is to prevent all jobs to run immediately after the scheduler is started.
    /// </summary>
    /// <remarks>Implementation will not work correctly, when scheduler is stopped and restarted.</remarks>
    public class SlowStartRestriction : IJobStartRestriction
    {
        private DateTime? _firstRun;
        
        /// <summary>
        /// Delay necessary for each job, before another job may be started.
        /// </summary>
        public TimeSpan StartDelay { get; } = TimeSpan.FromSeconds(10);
        
        /// <summary>
        /// TimeSpan considered as startup phase where SlowStartRestriction is active
        /// </summary>
        public TimeSpan StartupPhase { get; } = TimeSpan.FromMinutes(2);

        /// <inheritdoc />
        public bool RestrictStart(string jobToStart, IEnumerable<string> runningJobs)
        {
            var utcNow = DateTime.UtcNow;
            if (_firstRun == null)
            {
                // This is not the exact time, when the first job is started but almost.
                _firstRun = utcNow;
            }

            if (!(utcNow - _firstRun < StartupPhase))
            {
                // Only used during startupPhase
                return false;
            }

            var neededDelay = Multiply(StartDelay, runningJobs.Count());
            return utcNow - _firstRun < neededDelay;
        }

        private TimeSpan Multiply(TimeSpan input, int factor)
        {
            // Multiply not available in .NET 4.7.2
            return TimeSpan.FromTicks(input.Ticks * factor);
        }
    }
}