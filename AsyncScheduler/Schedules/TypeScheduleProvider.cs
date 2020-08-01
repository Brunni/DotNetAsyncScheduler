using System;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Provides a ScheduleProvider for the given ScheduleType
    /// </summary>
    internal class TypeScheduleProvider : IScheduleProvider
    {
        private readonly Type _scheduleType;
        private readonly IServiceProvider _serviceProvider;

        internal TypeScheduleProvider(Type scheduleType, IServiceProvider serviceProvider)
        {
            _scheduleType = scheduleType;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public ISchedule GetSchedule()
        {
            return (ISchedule) _serviceProvider.GetService(_scheduleType);
        }
    }
}