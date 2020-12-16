using System.Threading.Tasks;

namespace AsyncScheduler
{
    /// <summary>
    /// Interface which can be implemented by a Job,
    /// so its shutdown method is executed, when scheduler is stopped by CancellationToken.
    /// </summary>
    public interface IShutdownJob : IJob
    {

        /// <summary>
        /// Method is executed on shutdown of Scheduler/Job independent whether this job was started before or not.
        /// It is allowed to throw an exception as they are caught.
        /// </summary>
        /// <returns>message for Log</returns>
        Task<string> Shutdown();
    }
}