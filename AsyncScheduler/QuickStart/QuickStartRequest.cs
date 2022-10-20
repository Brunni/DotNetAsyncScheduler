using System.Threading;
using System.Threading.Tasks;

namespace AsyncScheduler.QuickStart
{
    /// <summary>
    /// Quick start allows jobs to be added (e.g. via UI), which are triggered in the next run (when no restrictions apply).
    /// A request can be cancelled by the user via cancellationToken.
    /// </summary>
    internal class QuickStartRequest
    {
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Requested job
        /// </summary>
        /// <remarks>returns null, if request was cancelled</remarks>
        public string? JobKey => _cancellationToken.IsCancellationRequested ? null : _jobKey;
        
        private readonly string _jobKey;

        private readonly TaskCompletionSource<QuickStartResult> _taskCompletionSource = new();

        public QuickStartRequest(string jobKey, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _jobKey = jobKey;
        }

        /// <summary>
        /// Blocking task for the caller to see if QuickStartRequest was successful
        /// </summary>
        public Task<QuickStartResult> CompletionTask => _taskCompletionSource.Task;

        /// <summary>
        /// Mark quick start as done. This resolves the Task for the caller.
        /// </summary>
        /// <param name="success">true, when started; false, when not started (e.g. restrictions)</param>
        public void MarkExecution(QuickStartResult success)
        {
            _taskCompletionSource.SetResult(success);
        }
    }
}