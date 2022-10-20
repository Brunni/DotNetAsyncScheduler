using System;

namespace AsyncScheduler.History
{
    /// <summary>
    /// Result of a job execution in the job history
    /// </summary>
    public interface IJobHistoryEntry
    {
        /// <summary>
        /// Execution time
        /// </summary>
        DateTimeOffset ExecutionTime { get; }

        /// <summary>
        /// Key of the job
        /// </summary>
        string JobKey { get; }

        /// <summary>
        /// String representation of the result
        /// </summary>
        string ResultString { get; }

        /// <summary>
        /// Status of the job (e.g. success, failure)
        /// </summary>
        JobResult JobResult { get; }
    }
}