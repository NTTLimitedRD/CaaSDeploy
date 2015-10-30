using System.Threading.Tasks;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// Contract for script macros.
    /// </summary>
    public interface IMacro
    {
        /// <summary>
        /// Substitutes the tokens in supplied string.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <param name="input">The input string.</param>
        /// <returns>The substituted string</returns>
        Task<string> SubstituteTokensInString(RuntimeContext runtimeContext, TaskContext taskContext, string input);
    }
}
