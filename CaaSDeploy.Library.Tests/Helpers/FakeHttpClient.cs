using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using CaasDeploy.Library.Utilities;
using Moq;

namespace CaasDeploy.Library.Tests.Helpers
{
    public class FakeHttpClient
    {
        private Mock<IHttpClient> _fakeClient;
        private IDictionary<string, Queue<HttpResponseMessage>> _responses;

        public FakeHttpClient()
        {
            _responses = new Dictionary<string, Queue<HttpResponseMessage>>();
        }

        public void AddResponse(string relativeUrl, string fileName)
        {
            AddResponse(relativeUrl, fileName, HttpStatusCode.OK);
        }

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

            var sampleFolderLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Resources\Messages");
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
