using System.Collections.Generic;

namespace AsyncScheduler.Restrictions
{
    /// <summary>
    /// Restrict start of job, based on other running jobs.
    /// Can be used for Mutex, Load restriction
    /// </summary>
    public interface IJobStartRestriction
    {
        /// <summary>
        /// Allows to prevent start of certain jobs based on the currently running jobs.
        /// Method is executed immediately before the job would be started (After Scheduling).
        /// </summary>
        /// <param name="jobToStart">job to be started</param>
        /// <param name="runningJobs">already running jobs</param>
        /// <returns>true, if job start should be prohibited in this cycle</returns>
        bool RestrictStart(string jobToStart, IEnumerable<string> runningJobs);
    }
}