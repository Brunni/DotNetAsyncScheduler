namespace AsyncScheduler.History
{
    /// <summary>
    /// Interface for accessing the history and results of finished jobs.
    /// </summary>
    public interface IJobHistory
    {
        /// <summary>
        /// Add a finished job with its result
        /// </summary>
        /// <param name="historyEntry">result of job</param>
        void Add(IJobHistoryEntry historyEntry);

        /// <summary>
        /// Retrieve last result of a job execution (failing or success).
        /// </summary>
        /// <param name="jobKey">key of the job</param>
        /// <returns>null, if no finished execution, yet</returns>
        IJobHistoryEntry? GetLastJobResult(string jobKey);
        
        /// <summary>
        /// Retrieve last result of a successful job execution.
        /// </summary>
        /// <param name="jobKey">key of the job</param>
        /// <returns>null, if no successful execution, yet</returns>
        IJobHistoryEntry? GetLastSuccessfulJobResult(string jobKey);
    }
}