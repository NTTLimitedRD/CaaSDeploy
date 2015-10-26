using System;
using System.Management.Automation;

using DD.CBU.CaasDeploy.Library.Contracts;

namespace DD.CBU.CaasDeploy.PowerShell
{
    /// <summary>
    /// A <see cref="ILogProvider" /> implementation for the PowerShell commands.
    /// </summary>
    internal class PowerShellLogProvider : ILogProvider
    {
        /// <summary>
        /// The cmdlet instance.
        /// </summary>
        PSCmdlet _cmdlet;

        /// <summary>
        /// Initializes a new instance of the <see cref="PowerShellLogProvider"/> class.
        /// </summary>
        /// <param name="cmdlet">The cmdlet instance.</param>
        public PowerShellLogProvider(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogMessage(string message)
        {
            _cmdlet.WriteObject(message);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogError(string message)
        {
            _cmdlet.WriteWarning(message);
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void LogException(Exception exception)
        {
            _cmdlet.WriteError(new ErrorRecord(exception, String.Empty, ErrorCategory.NotSpecified, null));
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
        /// <exception cref="System.NotImplementedException"></exception>
        public void CompleteProgress()
        {
        }
    }
}
