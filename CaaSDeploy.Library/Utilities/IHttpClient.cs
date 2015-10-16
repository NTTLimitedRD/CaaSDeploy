using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CaasDeploy.Library.Utilities
{
	/// <summary>
	/// Represents a type that can make HttpClient calls.
	/// </summary>
	public interface IHttpClient : IDisposable
	{
        /// <summary>
        /// Performs an asynchronous GET request.
        /// </summary>
        /// <param name="uri">The URI</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task<HttpResponseMessage> GetAsync(string uri);

        /// <summary>
        /// Performs an asynchronous POST request.
        /// </summary>
        /// <param name="uri">The URI</param>
        /// <param name="content">The content to post</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task<HttpResponseMessage> PostAsync(string uri, HttpContent content);
	}
}