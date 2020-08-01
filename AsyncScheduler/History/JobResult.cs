namespace AsyncScheduler.History
{
    public enum JobResult
    {
        /// <summary>
        /// Successful execution (no exception)
        /// </summary>
        Success = 1,
        
        /// <summary>
        ///  Exception was thrown during execution.
        /// </summary>
        Failure = 2
    }
}