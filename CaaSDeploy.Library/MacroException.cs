using System;

namespace DD.CBU.CaasDeploy.Library
{
    /// <summary>
    /// An exception which is thrown when errors in a macro occur.
    /// </summary>
    public sealed class MacroException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MacroException"/> class.
        /// </summary>
        public MacroException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MacroException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public MacroException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
