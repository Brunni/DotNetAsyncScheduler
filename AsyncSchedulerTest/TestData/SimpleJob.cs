using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;

namespace AsyncSchedulerTest.TestData
{
    public class SimpleJob : IJob
    {
        private int _executionCount = 0;

        public int ExecutionCount => _executionCount;

        public async Task<object> Start(CancellationToken cancellationToken)
        {
            await Task.Delay(200, cancellationToken);
            Interlocked.Increment(ref _executionCount);
            return await Task.FromResult(ExecutionCount);
        }
    }
}