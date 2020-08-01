using System;

namespace AsyncScheduler
{
    /// <summary>
    /// Simple activator class for Jobs, Schedules.
    /// Can be used, when no DI framework is in use.
    /// </summary>
    public class SimpleActivator : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }
    }
}