using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncScheduler.Restrictions
{
    /// <summary>
    /// Example implementation for restricting some jobs to run in parallel.
    /// The implementation is very basic.
    /// </summary>
    public class MutexRestriction : IJobStartRestriction
    {
        private readonly List<string> _exclusiveJobGroup;
        
        public MutexRestriction(params Type[] exclusiveJobGroup) : this(exclusiveJobGroup.Select(type => type.FullName).ToArray())
        {
        }

        public MutexRestriction(params string[] exclusiveJobs)
        {
            _exclusiveJobGroup = exclusiveJobs.ToList();
        }

        /// <inheritdoc />
        public bool RestrictStart(string jobToStart, IEnumerable<string> runningJobs)
        {
            if (!_exclusiveJobGroup.Contains(jobToStart))
            {
                return false;
            }

            bool isJobFromGroupAlreadyRunning = runningJobs.Any(j => _exclusiveJobGroup.Contains(j));
            return isJobFromGroupAlreadyRunning;
        }
    }
}