using System;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// Implementations of this interface provide logging capabilities.
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogMessage(string message);

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogError(string message);

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void LogException(Exception exception);

        /// <summary>
        /// Increments the progress.
        /// </summary>
        void IncrementProgress();

        /// <summary>
        /// Completes the progress.
        /// </summary>
        void CompleteProgress();
    }
}
