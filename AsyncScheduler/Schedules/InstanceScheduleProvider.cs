using System;

namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// A ScheduleProvider for a given schedule instance
    /// </summary>
    internal class InstanceScheduleProvider : IScheduleProvider
    {
        private readonly ISchedule _schedule;

        internal InstanceScheduleProvider(ISchedule schedule)
        {
            _schedule = schedule;
        }

        /// <inheritdoc />
        public ISchedule GetSchedule()
        {
            return _schedule;
        }
    }
}