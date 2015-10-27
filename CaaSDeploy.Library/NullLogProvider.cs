using System;

using DD.CBU.CaasDeploy.Library.Contracts;

namespace DD.CBU.CaasDeploy.Library
{
    /// <summary>
    /// A <see cref="ILogProvider"/> implementation which ignores all logs.
    /// </summary>
    public sealed class NullLogProvider : ILogProvider
    {
        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogMessage(string message)
        {
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogError(string message)
        {
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void LogException(Exception exception)
        {
        }

        /// <summary>
        /// Increments the progress.
        /// </summary>
        public void IncrementProgress()
        {
        }

        /// <summary>
        /// Completes the progress.
        /// </summary>
        public void CompleteProgress()
        {
        }
    }
}
