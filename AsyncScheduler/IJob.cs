using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncScheduler
{
    /// <summary>
    /// Interface to be implemented by each job.
    /// The job is instantiated via DI for each execution, so it is possible to inject e.g. database connection or current configuration values.
    /// 
    /// </summary>
    /// <remarks>implementation may implement <see cref="IDisposable"/> interface</remarks>
    public interface IJob
    {
        /// <summary>
        /// (Async) execution method of the job.
        /// </summary>
        /// <param name="cancellationToken">needs to stop execution and either throw OperationCanceledException or return immediately</param>
        /// <returns>return value is saved in job history and logged with ToString() method</returns>
        Task<object> Start(CancellationToken cancellationToken);
    }
}