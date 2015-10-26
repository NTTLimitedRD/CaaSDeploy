using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Utilities
{
    /// <summary>
    /// Provides commonly used helper methods to replace tokens in JSON templates.
    /// </summary>
    public static class TokenHelper
    {
        /// <summary>
        /// The parameter regex
        /// </summary>
        private static readonly Regex ParameterRegex = new Regex("\\$parameters\\['([^']*)'\\]");

        /// <summary>
        /// The resource property regex
        /// </summary>
        private static readonly Regex ResourcePropertyRegex = new Regex("\\$resources\\['([^']*)'\\]\\.([A-Za-z0-9\\.]+)");

        /// <summary>
        /// Substitutes the tokens in supplied JSON object.
        /// </summary>
        /// <param name="resourceDefinition">The resource definition.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="resourcesProperties">The resources properties.</param>
        public static void SubstituteTokensInJObject(JObject resourceDefinition, IDictionary<string, string> parameters, IDictionary<string, JObject> resourcesProperties)
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

        /// <summary>
        /// Substitutes the property tokens in the supplied string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The substituted string</returns>
        public static string SubstitutePropertyTokensInString(string input, IDictionary<string, string> parameters)
        {
            var paramsMatches = ParameterRegex.Matches(input);
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

        /// <summary>
        /// Substitutes the resource tokens in the supplied string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="resourcesProperties">The resources properties.</param>
        /// <returns>The substituted string</returns>
        public static string SubstituteResourceTokensInString(string input, IDictionary<string, JObject> resourcesProperties)
        {
            string output = input;
            if (resourcesProperties != null)
            {
                var resourceMatches = ResourcePropertyRegex.Matches(input);
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
