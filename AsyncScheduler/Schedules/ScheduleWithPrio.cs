namespace AsyncScheduler.Schedules
{
    /// <summary>
    /// Common interface for predefined Schedules with Custom Priority
    /// </summary>
    public interface IScheduleWithPrio : ISchedule
    {
        /// <summary>
        /// Priority used, when Schedule is active.
        /// </summary>
        int Priority { get; set; }

    }
}