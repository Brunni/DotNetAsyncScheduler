using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AsyncSchedulerTest.TestUtils
{
    public class XUnitLogger<T> : ILogger<T>, IDisposable
    {
        private readonly ITestOutputHelper _output;

        public XUnitLogger(ITestOutputHelper output)
        {
            _output = output;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _output.WriteLine($"[{logLevel}]: {state} {exception}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}