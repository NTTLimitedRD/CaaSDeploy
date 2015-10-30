using System.Net.Http;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
    /// <summary>
    /// Provides commonly used extension methods for <see cref="HttpResponseMessage"/> instances.
    /// </summary>
    internal static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Throws an exception if the supplied HTTP response message represents a failure.
        /// </summary>
        /// <param name="response">The response message.</param>
        public static void ThrowForHttpFailure(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync().Result;
                throw new CaasException(responseBody);
            }
        }
    }
}
