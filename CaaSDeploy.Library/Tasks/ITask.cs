using System.Threading.Tasks;

using CaasDeploy.Library.Models;

namespace CaasDeploy.Library.Tasks
{
    /// <summary>
    /// Abstraction for tasks perfomed as part of a deployment.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="context">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        Task Execute(TaskContext context);
    }
}