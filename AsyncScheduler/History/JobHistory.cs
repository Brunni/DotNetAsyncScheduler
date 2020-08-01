using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AsyncScheduler.History
{
    /// <summary>
    /// Holds the history of all finished job executions.
    /// </summary>
    public class JobHistory : IJobHistory
    {
        private readonly List<IJobHistoryEntry> _jobHistory = new List<IJobHistoryEntry>();

        /// <summary>
        /// Last execution for each job
        /// </summary>
        /// <remarks>Allows efficient access for scheduling</remarks>
        private readonly ConcurrentDictionary<string, IJobHistoryEntry> _lastExecutions =
            new ConcurrentDictionary<string, IJobHistoryEntry>();

        /// <summary>
        /// Last successful execution for each job
        /// </summary>
        /// <remarks>Allows efficient access for scheduling</remarks>
        private readonly ConcurrentDictionary<string, IJobHistoryEntry> _lastSuccessfulExecutions =
            new ConcurrentDictionary<string, IJobHistoryEntry>();

        /// <inheritdoc />
        public void Add(IJobHistoryEntry historyEntry)
        {
            _jobHistory.Add(historyEntry);
            _lastExecutions[historyEntry.JobKey] = historyEntry;
            if (historyEntry.JobResult == JobResult.Success)
            {
                _lastSuccessfulExecutions[historyEntry.JobKey] = historyEntry;
            }
        }

        /// <inheritdoc />
        public IJobHistoryEntry GetLastJobResult(string jobKey)
        {
            _lastExecutions.TryGetValue(jobKey, out var entry);
            return entry;
        }

        /// <inheritdoc />
        public IJobHistoryEntry GetLastSuccessfulJobResult(string jobKey)
        {
            _lastSuccessfulExecutions.TryGetValue(jobKey, out var entry);
            return entry;
        }
    }
}