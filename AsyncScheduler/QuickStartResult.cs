namespace AsyncScheduler
{
    /// <summary>
    /// Enum for result of quick start request
    /// </summary>
    public enum QuickStartResult
    {
        Unknown = 0,
        
        /// <summary>
        /// Job was triggered
        /// </summary>
        Started = 1,
        
        /// <summary>
        /// Job was already running
        /// </summary>
        AlreadyRunning = 2,
        
        /// <summary>
        /// Job start was restricted by a JobRestriction
        /// </summary>
        Restricted = 3
    }
}