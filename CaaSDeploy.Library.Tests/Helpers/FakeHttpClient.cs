using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Utilities;
using Moq;

namespace DD.CBU.CaasDeploy.Library.Tests.Helpers
{
    /// <summary>
    /// Replaces the default <see cref="IHttpClient"/> instance with a mock and handles fake requests and responses.
    /// </summary>
    public class FakeHttpClient
    {
        /// <summary>
        /// The response dictionary.
        /// </summary>
        private readonly IDictionary<string, Queue<HttpResponseMessage>> _responses;

        /// <summary>
        /// The fake HTTP client mock.
        /// </summary>
        private Mock<IHttpClient> _fakeClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeHttpClient"/> class.
        /// </summary>
        public FakeHttpClient()
        {
            _responses = new Dictionary<string, Queue<HttpResponseMessage>>();
        }

        /// <summary>
        /// Adds a fake response.
        /// </summary>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="fileName">Name of the response file.</param>
        public void AddResponse(string relativeUrl, string fileName)
        {
            AddResponse(relativeUrl, fileName, HttpStatusCode.OK);
        }

        /// <summary>
        /// Adds a fake response.
        /// </summary>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="fileName">Name of the response file.</param>
        /// <param name="statusCode">The response status code.</param>
        public void AddResponse(string relativeUrl, string fileName, HttpStatusCode statusCode)
        {
            if (_fakeClient == null)
            {
                _fakeClient = new Mock<IHttpClient>();
                _fakeClient.Setup(mock => mock.Dispose());
                _fakeClient.Setup(mock => mock.GetAsync(It.IsAny<string>())).Returns(new Func<string, Task<HttpResponseMessage>>(url => GetMessage(url)));
                _fakeClient.Setup(mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns(new Func<string, HttpContent, Task<HttpResponseMessage>>((url, content) => GetMessage(url)));
                HttpClientFactory.FakeClient = _fakeClient.Object;
            }

            var sampleFolderLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Resources\Responses");
            var targetFile = Path.Combine(sampleFolderLocation, fileName);
            var contents = File.ReadAllText(targetFile);
            var message = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(contents, Encoding.UTF8, "application/json")
            };

            if (!_responses.ContainsKey(relativeUrl))
            {
                _responses.Add(relativeUrl, new Queue<HttpResponseMessage>());
            }

            _responses[relativeUrl].Enqueue(message);
        }

        /// <summary>
        /// Gets the fake response message for the supplied absolute URL.
        /// </summary>
        /// <param name="absoluteUrl">The absolute URL.</param>
        /// <returns>The fake response message.</returns>
        private Task<HttpResponseMessage> GetMessage(string absoluteUrl)
        {
            Console.WriteLine(absoluteUrl);

            var relativeUrl = _responses.Keys.FirstOrDefault(url => absoluteUrl.EndsWith(url));
            if (relativeUrl == null)
            {
                throw new Exception($"No fake HTTP response found for '{absoluteUrl}'.");
            }

            var response = _responses[relativeUrl].Dequeue();
            if (response == null)
            {
                throw new Exception($"No fake HTTP response found for '{absoluteUrl}'.");
            }

            return Task.FromResult(response);
        }
    }
}
