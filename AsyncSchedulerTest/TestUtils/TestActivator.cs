using System;
using AsyncSchedulerTest.TestData;

namespace AsyncSchedulerTest.TestUtils
{
    public class TestActivator : IServiceProvider
    {
        private readonly SimpleJob? _simpleJobInstance;
        private readonly ShutdownJob? _shutdownJobInstance;

        public TestActivator(SimpleJob? simpleJobInstance = null, ShutdownJob? shutdownJobInstance = null)
        {
            _simpleJobInstance = simpleJobInstance;
            _shutdownJobInstance = shutdownJobInstance;
        }

        public object GetService(Type serviceType)
        {
            // Simulate DI issue
            if (serviceType == (typeof(NotImplementedSchedule)))
            {
                return new NotImplementedSchedule("DI");
            }

            // Always return same instance to allow counting of calls
            if (serviceType == typeof(SimpleJob) && _simpleJobInstance != null)
            {
                return _simpleJobInstance;
            }
            
            if (serviceType == typeof(ShutdownJob) && _shutdownJobInstance != null)
            {
                return _shutdownJobInstance;
            }
            return Activator.CreateInstance(serviceType) ?? throw new InvalidOperationException("Unable to create object for type: " + serviceType);
        }
    }
}