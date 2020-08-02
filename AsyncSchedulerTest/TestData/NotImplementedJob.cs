using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;

namespace AsyncSchedulerTest.TestData
{
    public class NotImplementedJob : IJob
    {
        public Task<object> Start(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}