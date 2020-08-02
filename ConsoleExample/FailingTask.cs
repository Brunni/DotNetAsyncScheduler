using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using Microsoft.Extensions.Logging;

namespace ConsoleExample
{
    public class FailingTask : IJob
    {
        private readonly ILogger<FailingTask> _logger;

        public FailingTask(ILogger<FailingTask> logger)
        {
            _logger = logger;
        }

        public async Task<object> Start(CancellationToken cancellationToken)
        {
            try
            {
                var httpClient = new HttpClient();
                _logger.LogInformation("Starting download ...");
                await httpClient.GetAsync(
                    //"http://dl.minio.io/server/minio/release/windows-amd64/archive/minio.RELEASE.2019-10-12T01-39-57Z",
                    "http://localhost:123/asdf",
                    cancellationToken);
                _logger.LogInformation("Download finished ...");
            }
            catch (OperationCanceledException e)
            {
                _logger.LogInformation(e,"Download cancelled but we return 0 and mark job as success");
                //Hint: If we return a value here, the value is recorded. If we rethrow, the result is not recorded/logged.
                return 0;
            }

            return 1;
        }
    }
}