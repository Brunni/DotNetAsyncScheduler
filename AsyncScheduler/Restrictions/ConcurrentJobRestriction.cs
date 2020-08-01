using System.Collections.Generic;
using System.Linq;

namespace AsyncScheduler.Restrictions
{
    /// <summary>
    /// Example implementation of a restriction to 
    /// restrict the maximum number of parallel running jobs.
    /// </summary>
    public sealed class ConcurrentJobRestriction : IJobStartRestriction
    {
        /// <summary>
        /// Maximum number of parallel jobs
        /// </summary>
        public int MaximumParallelJobs { get; set; } = 1;

        /// <inheritdoc />
        public bool RestrictStart(string jobToStart, IEnumerable<string> runningJobs)
        {
            return runningJobs.ToList().Count >= MaximumParallelJobs;
        }
    }
}