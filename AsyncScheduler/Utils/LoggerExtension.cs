using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AsyncScheduler.Utils
{
    /// <summary>
    /// Extension for <see cref="ILogger"/> interface
    /// </summary>
    internal static class LoggerExtension
    {
        /// <summary>
        /// Pushes scope to the logger for structured logging.
        /// e.g. server=firstServer
        /// </summary>
        /// <param name="logger">this</param>
        /// <param name="key">key of context value</param>
        /// <param name="value">context value</param>
        /// <returns>disposable for the scope</returns>
        internal static IDisposable BeginScope(this ILogger logger, string key, string value)
        {
            return logger.BeginScope(new Dictionary<string, object>{
                [key] = value
            });
        }
    }
}