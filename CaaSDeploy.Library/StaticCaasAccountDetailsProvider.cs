using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;

namespace DD.CBU.CaasDeploy.Library
{
    /// <summary>
    /// Gets an instance of <see cref="CaasAccountDetails"/> from supplied credentials.
    /// </summary>
    public sealed class StaticCaasAccountDetailsProvider : ICaasAccountDetailsProvider
    {
        /// <summary>
        /// The account details
        /// </summary>
        private readonly CaasAccountDetails _accountDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticCaasAccountDetailsProvider"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="orgId">The org identifier.</param>
        /// <param name="baseUrl">The base URL.</param>
        public StaticCaasAccountDetailsProvider(string userName, string password, string orgId, string baseUrl)
        {
            _accountDetails = new CaasAccountDetails
            {
                UserName = userName,
                Password = password,
                OrgId = orgId,
                BaseUrl = baseUrl
            };
        }

        /// <summary>
        /// Gets the account details.
        /// </summary>
        /// <returns>
        /// The account details.
        /// </returns>
        public async Task<CaasAccountDetails> GetAccountDetails()
        {
            return await Task.FromResult(_accountDetails);
        }
    }
}
