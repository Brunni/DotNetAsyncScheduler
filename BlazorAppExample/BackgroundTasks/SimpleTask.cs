using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using Microsoft.Extensions.Logging;

namespace BlazorAppExample.BackgroundTasks
{
    public class SimpleTask : IJob
    {
        private readonly ILogger<SimpleTask> _logger;

        public SimpleTask(ILogger<SimpleTask> logger)
        {
            _logger = logger;
        }

        public async Task<object> Start(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Simple task running");
            await Task.Delay(TimeSpan.FromSeconds(6), cancellationToken);
            return 1;
        }
    }
}