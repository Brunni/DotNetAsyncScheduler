using System.Collections.Concurrent;

namespace AsyncScheduler.History
{
    /// <summary>
    /// Holds the history of all finished job executions.
    /// </summary>
    public class JobHistory : IJobHistory
    {
        // private readonly List<IJobHistoryEntry>  _jobHistory = new();

        /// <summary>
        /// Last execution for each job
        /// </summary>
        /// <remarks>Allows efficient access for scheduling</remarks>
        private readonly ConcurrentDictionary<string, IJobHistoryEntry> _lastExecutions = new();

        /// <summary>
        /// Last successful execution for each job
        /// </summary>
        /// <remarks>Allows efficient access for scheduling</remarks>
        private readonly ConcurrentDictionary<string, IJobHistoryEntry> _lastSuccessfulExecutions = new();

        /// <inheritdoc />
        public void Add(IJobHistoryEntry historyEntry)
        {
            // _jobHistory.Add(historyEntry);
            _lastExecutions[historyEntry.JobKey] = historyEntry;
            if (historyEntry.JobResult == JobResult.Success)
            {
                _lastSuccessfulExecutions[historyEntry.JobKey] = historyEntry;
            }
        }

        /// <inheritdoc />
        public IJobHistoryEntry? GetLastJobResult(string jobKey)
        {
            _lastExecutions.TryGetValue(jobKey, out var entry);
            return entry;
        }

        /// <inheritdoc />
        public IJobHistoryEntry? GetLastSuccessfulJobResult(string jobKey)
        {
            _lastSuccessfulExecutions.TryGetValue(jobKey, out var entry);
            return entry;
        }
    }
}