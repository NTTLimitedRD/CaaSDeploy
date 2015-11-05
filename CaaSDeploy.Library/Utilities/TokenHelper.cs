using System.Collections.Generic;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Macros;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
    /// <summary>
    /// Provides commonly used helper methods to replace tokens in JSON templates.
    /// </summary>
    public static class TokenHelper
    {
        /// <summary>
        /// The macros to execute.
        /// </summary>
        private static readonly IList<IMacro> Macros = new List<IMacro>
        {
            new ParametersMacro(),
            new ResourcesMacro(),
            new ImageMacro()
        };

        /// <summary>
        /// Substitutes the tokens in supplied JSON object.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <param name="resourceDefinition">The resource definition.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public static async Task SubstituteTokensInJObject(RuntimeContext runtimeContext, TaskContext taskContext, JObject resourceDefinition)
        {
            foreach (var parameter in resourceDefinition)
            {
                if (parameter.Value is JObject)
                {
                    await SubstituteTokensInJObject(runtimeContext, taskContext, (JObject)parameter.Value);
                }
                else if (parameter.Value is JValue)
                {
                    var value = parameter.Value.Value<string>();
                    value = await SubstituteTokensInString(runtimeContext, taskContext, value);
                    parameter.Value.Replace(new JValue(value));
                }
                else if (parameter.Value is JArray)
                {
                    foreach (var jtoken in ((JArray)parameter.Value))
                    {
                        if (jtoken is JObject)
                        {
                            await SubstituteTokensInJObject(runtimeContext, taskContext, (JObject)jtoken);
                        }
                        else
                        {
                            var value = jtoken.Value<string>();
                            value = await SubstituteTokensInString(runtimeContext, taskContext, value);
                            ((JValue)jtoken).Replace(new JValue(value));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Substitutes the property tokens in the supplied string.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <param name="input">The input string.</param>
        /// <returns>The substituted string</returns>
        public static async Task<string> SubstituteTokensInString(RuntimeContext runtimeContext, TaskContext taskContext, string input)
        {
            var value = input;

            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            foreach (var macro in Macros)
            {
                value = await macro.SubstituteTokensInString(runtimeContext, taskContext, value);
            }

            return value;
        }
    }
}
