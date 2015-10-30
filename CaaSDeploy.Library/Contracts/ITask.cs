using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Models;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// Abstraction for tasks performed as part of a deployment.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task" />.</returns>
        Task Execute(RuntimeContext runtimeContext, TaskContext taskContext);
    }
}