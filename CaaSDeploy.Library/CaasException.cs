using System;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library
{
    /// <summary>
    /// Exception class for CaaS errors.
    /// </summary>
    public class CaasException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaasException" /> class.
        /// </summary>
        /// <param name="errorText">The error text.</param>
        public CaasException(string errorText) : base(GetMessage(errorText))
        {
            var jObject = JObject.Parse(errorText);
            this.FullResponse = jObject;
            this.Operation = jObject["operation"]?.Value<string>();
            this.ResponseCode = jObject["responseCode"]?.Value<string>();
            this.InfoMessages = ((JArray)jObject["info"])?.Select(jv => (string)jv).ToArray();
            this.WarningMessages = ((JArray)jObject["warning"])?.Select(jv => (string)jv).ToArray();
            this.ErrorMessages = ((JArray)jObject["error"])?.Select(jv => (string)jv).ToArray();
            this.RequestId = jObject["requestId"]?.Value<string>();
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <param name="errorText">The error text.</param>
        /// <returns>The error message.</returns>
        private static string GetMessage(string errorText)
        {
            var jObject = JObject.Parse(errorText);
            return jObject["message"]?.Value<string>();
        }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the information messages.
        /// </summary>
        public string[] InfoMessages { get; set; }

        /// <summary>
        /// Gets or sets the warning messages.
        /// </summary>
        public string[] WarningMessages { get; set; }

        /// <summary>
        /// Gets or sets the error messages.
        /// </summary>
        public string[] ErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the full response.
        /// </summary>
        public JObject FullResponse { get; set; }
    }
}
