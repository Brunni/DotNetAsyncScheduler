using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;

namespace ConsoleExample
{
    public class SimpleTask : IJob
    {
        public async Task<object> Start(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(6), cancellationToken);
            await Console.Out.WriteLineAsync("Task started");
            return 1;
        }
    }
}