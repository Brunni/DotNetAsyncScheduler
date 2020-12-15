using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;

namespace BlazorAppExample.BackgroundTasks
{
    public class RestrictedTask : IJob
    {
        public async Task<object> Start(CancellationToken cancellationToken)
        {
            return await Task.FromResult("Task might never get executed :/");
        }
    }
}