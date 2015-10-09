using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CaasDeploy.Library.Utilities
{
    public static class TokenHelper
    {
        private static Regex _parameterRegex = new Regex("\\$parameters\\['([^']*)'\\]");
        private static Regex _resourcePropertyRegex = new Regex("\\$resources\\['([^']*)'\\]\\.([A-Za-z0-9\\.]+)");

        public static void SubstituteTokensInJObject(JObject resourceDefinition, Dictionary<string, string> parameters, Dictionary<string, JObject> resourcesProperties)
        {
            foreach (var parameter in resourceDefinition)
            {
                if (parameter.Value is JObject)
                {
                    SubstituteTokensInJObject((JObject)parameter.Value, parameters, resourcesProperties);
                }
                else if (parameter.Value is JValue)
                {
                    string tokenValue = parameter.Value.Value<string>();

                    var newValue = SubstitutePropertyTokensInString(tokenValue, parameters);
                    newValue = SubstituteResourceTokensInString(newValue, resourcesProperties);
                    parameter.Value.Replace(new JValue(newValue));
                }
                else if (parameter.Value is JArray)
                {
                    foreach (var jtoken in ((JArray)parameter.Value))
                    {
                        SubstituteTokensInJObject((JObject)jtoken, parameters, resourcesProperties);
                    }

                }
            }
        }

        public static string SubstitutePropertyTokensInString(string input, Dictionary<string, string> parameters)
        {
            var paramsMatches = _parameterRegex.Matches(input);
            string output = input;
            if (paramsMatches.Count > 0)
            {
                foreach (Match paramsMatch in paramsMatches)
                {
                    string newValue = parameters[paramsMatch.Groups[1].Value];
                    output = output.Replace(paramsMatch.Groups[0].Value, newValue);
                }

            }
            return output;
        }

        public static string SubstituteResourceTokensInString(string input, Dictionary<string, JObject> resourcesProperties)
        {
            string output = input;
            if (resourcesProperties != null)
            {
                var resourceMatches = _resourcePropertyRegex.Matches(input);
                if (resourceMatches.Count > 0)
                {
                    foreach (Match resourceMatch in resourceMatches)
                    {
                        string resourceId = resourceMatch.Groups[1].Value;
                        string property = resourceMatch.Groups[2].Value;
                        var newValue = resourcesProperties[resourceId].SelectToken(property).Value<string>();
                        output = output.Replace(resourceMatch.Groups[0].Value, newValue);
                    }
                }
            }
            return output;
        }
    }
}
