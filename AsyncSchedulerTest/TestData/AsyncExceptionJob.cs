using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;

namespace AsyncSchedulerTest.TestData
{
    public class AsyncExceptionJob : IJob
    {
        public async Task<object> Start(CancellationToken cancellationToken)
        {
            await Task.Delay(20, cancellationToken);
            // Hint: This is a late exception, which is not caught during start of job but after finishing. 
            throw new Exception("Async exception");
        }
    }
}