using System;

namespace AsyncScheduler.History
{
    /// <inheritdoc />
    public class JobHistoryEntry : IJobHistoryEntry
    {
        /// <inheritdoc />
        public DateTime ExecutionTime { get; set; }
        
        /// <inheritdoc />
        public string JobKey { get; set; }
        
        /// <inheritdoc />
        public string ResultString => Result.ToString();
        
        /// <summary>
        /// Result object returned by task. (Might be used later.)
        /// </summary>
        public object Result { get; set; }
        
        /// <inheritdoc />
        public JobResult JobResult { get; set; }
    }
}