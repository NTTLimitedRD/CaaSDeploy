using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Models;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// Provides CaaS account information.
    /// </summary>
    public interface ICaasAccountDetailsProvider
    {
        /// <summary>
        /// Gets the account details.
        /// </summary>
        /// <returns>The account details.</returns>
        Task<CaasAccountDetails> GetAccountDetails();
    }
}
