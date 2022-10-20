using System;

namespace AsyncScheduler.History
{
    /// <inheritdoc />
    public class JobHistoryEntry : IJobHistoryEntry
    {
        public JobHistoryEntry(DateTimeOffset executionTime, string jobKey,
            JobResult jobResult, object? result = null)
        {
            ExecutionTime = executionTime;
            JobKey = jobKey ?? throw new ArgumentNullException(nameof(jobKey));
            Result = result;
            JobResult = jobResult;
        }

        /// <inheritdoc />
        public DateTimeOffset ExecutionTime { get; }

        /// <inheritdoc />
        public string JobKey { get; }

        /// <inheritdoc />
        public string ResultString => Result?.ToString() ?? "null";

        /// <summary>
        /// Result object returned by task. (Might be used later.)
        /// </summary>
        public object? Result { get; }

        /// <inheritdoc />
        public JobResult JobResult { get; }
    }
}