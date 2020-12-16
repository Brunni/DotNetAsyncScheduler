using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;

namespace AsyncSchedulerTest.TestData
{
    public class ShutdownJob : IShutdownJob
    {
        public int ShutdownCount { get; private set; } = 0;

        public async Task<object> Start(CancellationToken cancellationToken)
        {
            return await Task.FromResult("Done");
        }

        public Task<string> Shutdown()
        {
            ShutdownCount++;
            return Task.FromResult("Shutdown executed");
        }
    }
}