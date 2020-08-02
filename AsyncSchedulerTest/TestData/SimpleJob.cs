using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;

namespace AsyncSchedulerTest.TestData
{
    public class SimpleJob : IJob
    {
        public int ExecutionCount { get; private set; } = 0;

        public async Task<object> Start(CancellationToken cancellationToken)
        {
            await Task.Delay(200, cancellationToken);
            ExecutionCount++;
            return await Task.FromResult(ExecutionCount);
        }
    }
}