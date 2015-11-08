﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Utilities;

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
                return SubstituteTokensInString(input, taskContext.Parameters, true);
            });
        }

        /// <summary>
        /// Substitutes the property tokens in the supplied string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="required">A value indicating whether the parameter is required.</param>
        /// <returns>The substituted string</returns>
        internal static string SubstituteTokensInString(string input, IDictionary<string, string> parameters, bool required)
        {
            string output = input;
            MatchCollection paramsMatches = ParameterRegex.Matches(output);

            while (paramsMatches.Count > 0)
            {
                Match paramsMatch = paramsMatches[paramsMatches.Count - 1];
                string parameterValue;

                if (!parameters.TryGetValue(paramsMatch.Groups[1].Value, out parameterValue))
                {
                    if (required)
                    {
                        throw new TemplateParserException($"Value for parameter '{paramsMatch.Groups[1].Value}' has not been provided.");
                    }

                    parameterValue = string.Empty;
                }

                if (IsNested(output, paramsMatch))
                {
                    parameterValue = "'" + parameterValue + "'";
                }

                output = output.Replace(paramsMatch.Groups[0].Value, parameterValue);
                paramsMatches = ParameterRegex.Matches(output);
            }

            return output;
        }

        /// <summary>
        /// Checkes whether the replacement of the supplied token requires quotes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="match">The match.</param>
        /// <returns>True if quotes are required, otherwise false.</returns>
        private static bool IsNested(string input, Match match)
        {
            if ((match.Index == 0) || (match.Index + match.Value.Length == input.Length))
            {
                return false;
            }

            if ((input[match.Index - 1] == '[') || (input[match.Index + match.Value.Length] == ']'))
            {
                return true;
            }

            return false;
        }
    }
}
