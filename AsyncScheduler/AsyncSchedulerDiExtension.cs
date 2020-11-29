using AsyncScheduler.JobStorage;
using AsyncScheduler.Schedules;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncScheduler
{
    /// <summary>
    /// Allows registration of internal classes for DI.
    /// </summary>
    public static class AsyncSchedulerDiExtension
    {
        /// <summary>
        /// Register types to DI.
        /// </summary>
        /// <param name="services">this</param>
        /// <returns>services for fluent interface chaining</returns>
        public static IServiceCollection RegisterAsyncScheduler(this IServiceCollection services)
        {
            services.AddTransient<Scheduler>();
            services.AddTransient<ScheduleOnceWithRetryDelay>();
            services.AddTransient<ScheduleOnce>();
            services.AddTransient<ScheduleNever>();
            services.AddTransient<ISchedulerClock, UtcSchedulerClock>();
            services.AddTransient<JobManager>();
            services.AddTransient<IJobStorage, InMemoryStorage>();
            return services;
        }
    }
}