using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
	/// <summary>
	/// The HTTP client adapter.
	/// </summary>
	public class HttpClientAdapter : IDisposable, IHttpClient
	{
		/// <summary>
		/// The underlying <see cref="HttpClient"/>.
		/// </summary>
		private readonly HttpClient _client;

		/// <summary>
		/// Initialises a new instance of the <see cref="HttpClientAdapter"/> class. 
		/// Create a new <see cref="HttpClient"/> adaptor.
		/// </summary>
		/// <param name="client">
		/// The <see cref="HttpClient"/> wrapped by the adaptor.
		/// </param>
		public HttpClientAdapter(HttpClient client)
		{
			if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            _client = client;
		}

        /// <summary>
        /// Performs an asynchronous GET request.
        /// </summary>
        /// <param name="uri">The URI</param>
        /// <returns>The <see cref="Task" />.</returns>
        public async Task<HttpResponseMessage> GetAsync(string uri)
		{
			return await _client.GetAsync(uri);
		}

        /// <summary>
        /// Performs an asynchronous POST request.
        /// </summary>
        /// <param name="uri">The URI</param>
        /// <param name="content">The content to post</param>
        /// <returns>The <see cref="Task" />.</returns>
        public async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content)
		{
			return await _client.PostAsync(uri, content);
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
		{
			if (disposing)
            {
                _client.Dispose();
            }
        }
    }
}