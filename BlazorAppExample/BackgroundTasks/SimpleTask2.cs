using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using Microsoft.Extensions.Logging;

namespace BlazorAppExample.BackgroundTasks
{
    public class SimpleTask2 : IJob
    {
        private readonly ILogger<SimpleTask2> _logger;

        public SimpleTask2(ILogger<SimpleTask2> logger)
        {
            _logger = logger;
        }

        public async Task<object> Start(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Simple task2 running");
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            return 1;
        }
    }
}