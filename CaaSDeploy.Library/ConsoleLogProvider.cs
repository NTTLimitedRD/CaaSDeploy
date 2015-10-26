using System;

using CaasDeploy.Library.Contracts;

namespace CaasDeploy.Library
{
    /// <summary>
    /// A <see cref="ILogProvider"/> implementation which writes to the console.
    /// </summary>
    public sealed class ConsoleLogProvider : ILogProvider
    {
        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogMessage(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void LogException(Exception exception)
        {
            LogError(exception.ToString());
        }

        /// <summary>
        /// Increments the progress.
        /// </summary>
        public void IncrementProgress()
        {
            Console.ResetColor();
            Console.Write(".");
        }

        /// <summary>
        /// Completes the progress.
        /// </summary>
        public void CompleteProgress()
        {
            Console.ResetColor();
            Console.WriteLine("Done!");
        }
    }
}
