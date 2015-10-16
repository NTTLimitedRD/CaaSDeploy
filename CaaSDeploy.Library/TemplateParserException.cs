using System;

namespace CaasDeploy.Library
{
    /// <summary>
    /// An exception which is thrown when errors in a template are caught.
    /// </summary>
    public sealed class TemplateParserException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateParserException"/> class.
        /// </summary>
        public TemplateParserException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateParserException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TemplateParserException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateParserException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public TemplateParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
