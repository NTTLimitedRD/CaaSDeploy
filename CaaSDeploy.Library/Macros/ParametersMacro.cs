using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;

namespace DD.CBU.CaasDeploy.Library.Macros
{
    /// <summary>
    /// The parameters macro supports passing parameters to resources.
    /// </summary>
    public sealed class ParametersMacro : IMacro
    {
        /// <summary>
        /// The parameter regex
        /// </summary>
        private static readonly Regex ParameterRegex = new Regex("\\$parameters\\['([^']*)'\\]");

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
                var paramsMatches = ParameterRegex.Matches(input);
                var output = input;
                if (paramsMatches.Count > 0)
                {
                    foreach (Match paramsMatch in paramsMatches)
                    {
                        string parameterValue;

                        if (!taskContext.Parameters.TryGetValue(paramsMatch.Groups[1].Value, out parameterValue))
                        {
                            throw new TemplateParserException($"Value for parameter '{paramsMatch.Groups[1].Value}' has not been provided.");
                        }

                        output = output.Replace(paramsMatch.Groups[0].Value, parameterValue);
                    }
                }

                return output;
            });
        }
    }
}
