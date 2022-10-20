namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Common interface to provide a schedule for a job.
    /// Schedule providers allow schedules to be fetched from a database or file.
    /// </summary>
    /// <remarks>As schedules are fetched very often, when cycle delay is not increased. Consider caching when loading from file system.</remarks>
    public interface IScheduleProvider
    {
        /// <summary>
        /// Provides or instantiates the schedule.
        /// </summary>
        /// <returns>schedule</returns>
        ISchedule? GetSchedule();
    }
}