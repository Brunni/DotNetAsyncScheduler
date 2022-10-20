using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncScheduler;
using AsyncScheduler.Restrictions;
using AsyncScheduler.Schedules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ConsoleExample
{
    public class Program
    {
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            //Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddTransient<SimpleTask>()
                .AddTransient<ExampleTask2>()
                .AddTransient<FailingTask>()
                .AddTransient<EndlessLoopTask>()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true))
                .RegisterAsyncScheduler()
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            logger.LogDebug("Starting application");
            try
            {
                var scheduler = serviceProvider.GetService<Scheduler>();
                if (scheduler == null)
                {
                    throw new NullReferenceException("Unable to get Scheduler");
                }

                var jobManager = scheduler.JobManager;

                jobManager.AddJob<SimpleTask>(new TimeSlotSchedule
                {
                    StartTime = DateTime.UtcNow + TimeSpan.FromSeconds(20)
                });
                jobManager.AddJob<ExampleTask2>(new IntervalSchedule(TimeSpan.FromSeconds(25)));
                jobManager.AddJob<FailingTask>(new IntervalSchedule(TimeSpan.FromSeconds(10)));
                jobManager.AddJob<EndlessLoopTask, ScheduleOnce>();
                // jobManager.AddJob<FailingTask, ScheduleOnceWithRetryDelay>();

                //scheduler.AddRestriction(new JobRestriction());
                scheduler.AddRestriction(new ConcurrentJobRestriction
                {
                    MaximumParallelJobs = 10
                });
                scheduler.AddRestriction(new SlowStartRestriction());
                scheduler.AddRestriction(new MutexRestriction(typeof(SimpleTask), typeof(FailingTask)));
                Console.CancelKeyPress += (o, eventArgs) => ConsoleOnCancelKeyPress(o, eventArgs, logger);
                var schedulerTask = scheduler.Start(_cancellationTokenSource.Token);
                Task.Delay(TimeSpan.FromSeconds(40))
                     .ContinueWith(t => _cancellationTokenSource.Cancel())
                    .ContinueWith(t => jobManager.RemoveJob<SimpleTask>())
                    .ContinueWith(async t => await Task.Delay(TimeSpan.FromSeconds(10))
                        .ContinueWith(
                            t2 => jobManager.AddJob<SimpleTask>(new IntervalSchedule(TimeSpan.FromSeconds(10))))
                    );

                schedulerTask.Wait();

                // Scheduler can be restarted
                _cancellationTokenSource = new CancellationTokenSource();
                scheduler.Start(_cancellationTokenSource.Token).Wait();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in Program");
            }
            finally
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                Log.CloseAndFlush();
            }
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e, ILogger logger)
        {
            logger.LogInformation("Cancel Key was pressed. CancellationToken will be cancelled");
            _cancellationTokenSource.Cancel();
        }
    }
}