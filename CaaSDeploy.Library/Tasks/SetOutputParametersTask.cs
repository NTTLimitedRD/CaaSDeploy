using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which writes the output parameters to the context.
    /// </summary>
    public sealed class SetOutputParametersTask : ITask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetOutputParametersTask"/> class.
        /// </summary>
        /// <param name="outputParameters">The defined output parameters.</param>
        public SetOutputParametersTask(JObject outputParameters)
        {
            if (outputParameters == null)
            {
                throw new ArgumentNullException(nameof(outputParameters));
            }

            OutputParameters = outputParameters;
        }

        /// <summary>
        /// Gets the defined output parameters.
        /// </summary>
        public JObject OutputParameters { get; private set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(RuntimeContext runtimeContext, TaskContext taskContext)
        {
            taskContext.OutputParameters = new Dictionary<string, string>();

            foreach (var param in OutputParameters.Properties())
            {
                var value = param.Value["value"].Value<string>();
                value = await TokenHelper.SubstituteTokensInString(runtimeContext, taskContext, value);
                taskContext.OutputParameters.Add(param.Name, value);
            }
        }
    }
}
