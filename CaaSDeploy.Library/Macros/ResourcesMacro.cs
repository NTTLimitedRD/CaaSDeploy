using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Macros
{
    /// <summary>
    /// The resources marco provides lookup functionality between resources.
    /// </summary>
    public sealed class ResourcesMacro : IMacro
    {
        /// <summary>
        /// The resource property regex
        /// </summary>
        private static readonly Regex ResourcePropertyRegex = new Regex("\\$resources\\['([^']*)'\\]\\.([A-Za-z0-9\\.]+)");

        /// <summary>
        /// Substitutes the property tokens in the supplied string.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <param name="input">The input string.</param>
        /// <returns>The substituted string</returns>
        public async Task<string> SubstituteTokensInString(RuntimeContext runtimeContext, TaskContext taskContext, string input)
        {
            return await Task.Run(() =>
            {
                string output = input;

                if (taskContext.ResourcesProperties != null)
                {
                    MatchCollection resourceMatches = ResourcePropertyRegex.Matches(output);

                    while (resourceMatches.Count > 0)
                    {
                        Match resourceMatch = resourceMatches[resourceMatches.Count - 1];
                        string resourceId = resourceMatch.Groups[1].Value;
                        string property = resourceMatch.Groups[2].Value;

                        JObject resource;

                        if (!taskContext.ResourcesProperties.TryGetValue(resourceId, out resource))
                        {
                            throw new TemplateParserException($"Referenced resource '{resourceId}' not found.");
                        }

                        var newValue = resource.SelectToken(property).Value<string>();

                        if (TokenHelper.QuotesRequired(output, resourceMatch))
                        {
                            newValue = "'" + newValue + "'";
                        }

                        output = output.Replace(resourceMatch.Groups[0].Value, newValue);
                        resourceMatches = ResourcePropertyRegex.Matches(output);
                    }
                }

                return output;
            });
        }
    }
}
