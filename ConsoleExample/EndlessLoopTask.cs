using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using Microsoft.Extensions.Logging;

namespace ConsoleExample
{
    public class EndlessLoopTask : IJob
    {
        private ILogger<EndlessLoopTask> _logger;

        public EndlessLoopTask(ILogger<EndlessLoopTask> logger)
        {
            _logger = logger;
        }

        public async Task<object> Start(CancellationToken cancellationToken)
        {
            long i = 1;
            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // We don't pass cancellation token here for testing reasons to get exception
                    await Task.Delay(10);
                    //Console.Write("i");
                    i++;
                }
                catch (OperationCanceledException e)
                {
                    _logger.LogInformation(e, "Stopping task");
                    Console.WriteLine("EndlessLoopTask stopped");
                    throw;
                }
            }
            return i;
        }
    }
}