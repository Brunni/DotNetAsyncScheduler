using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;

namespace ConsoleExample
{
    public class ExampleTask2 : IJob
    {
        public async Task<object> Start(CancellationToken cancellationToken)
        {
            Console.WriteLine("ExampleTask2 started");
            await Task.Delay(TimeSpan.FromSeconds(8), cancellationToken);
            Console.WriteLine("ExampleTask2 finished");
            return 0;
        }
    }
}