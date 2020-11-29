using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using Microsoft.Extensions.Logging;

namespace BlazorAppExample.BackgroundTasks
{
    public class EndlessLoopTask : IJob
    {
        private readonly ILogger<EndlessLoopTask> _logger;

        public EndlessLoopTask(ILogger<EndlessLoopTask> logger)
        {
            _logger = logger;
        }

        public async Task<object> Start(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    _logger.LogInformation("EndlessLoopTask is still doing stuff");
                    cancellationToken.ThrowIfCancellationRequested();
                    // We don't pass cancellation token here for testing reasons to get exception
                    await Task.Delay(1000);
                    //Console.Write("i");
                }
                catch (OperationCanceledException e)
                {
                    _logger.LogInformation(e, "Stopping task");
                    Console.WriteLine("EndlessLoopTask stopped");
                    throw;
                }
            }
        }
    }
}