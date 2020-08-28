using System;
using AsyncSchedulerTest.TestData;

namespace AsyncSchedulerTest.TestUtils
{
    public class TestActivator : IServiceProvider
    {
        private readonly SimpleJob _simpleJobInstance;

        public TestActivator(SimpleJob simpleJobInstance = null)
        {
            _simpleJobInstance = simpleJobInstance;
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
            return Activator.CreateInstance(serviceType);
        }
    }
}