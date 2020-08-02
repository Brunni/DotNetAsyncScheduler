using System;
using JetBrains.Annotations;

namespace AsyncScheduler.History
{
    /// <inheritdoc />
    public class JobHistoryEntry : IJobHistoryEntry
    {
        public JobHistoryEntry(DateTime executionTime, [NotNull] string jobKey,
            JobResult jobResult, object result = null)
        {
            ExecutionTime = executionTime;
            JobKey = jobKey ?? throw new ArgumentNullException(nameof(jobKey));
            Result = result;
            JobResult = jobResult;
        }

        /// <inheritdoc />
        public DateTime ExecutionTime { get; }

        /// <inheritdoc />
        [NotNull]
        public string JobKey { get; }

        /// <inheritdoc />
        public string ResultString => Result?.ToString();

        /// <summary>
        /// Result object returned by task. (Might be used later.)
        /// </summary>
        [CanBeNull]
        public object Result { get; }

        /// <inheritdoc />
        public JobResult JobResult { get; }
    }
}