using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    public class CaasException : Exception
    {
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

        private static string GetMessage(string errorText)
        {
            var jObject = JObject.Parse(errorText);
            return jObject["message"]?.Value<string>();

        }

        public string Operation { get; set; }
        public string ResponseCode { get; set; }
        public string[] InfoMessages { get; set; }
        public string[] WarningMessages { get; set; }
        public string[] ErrorMessages { get; set; }
        public string RequestId { get; set; }
        public JObject FullResponse { get; set; }
    }
}
