using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using CaasDeploy.Api.Models;
using CaasDeploy.Library.Models;

namespace CaasDeploy.Library.Tests
{
    [TestClass]
    public class ApiTests
    {

        [TestMethod]
        public async Task CanSubmitDeploymentAndPollResults()
        {
            int jobId = await SubmitDeployment();
            int tries = 0;
            while (tries++ < 100)
            {
                var status = await PollForResults(jobId);
                if (status.status == "Succeeded")
                {
                    var result = JObject.Parse((string)status.result);
                    Assert.AreEqual("Success", result["status"].Value<string>());
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            Assert.Fail("Deployment timed out");
        }

        [TestMethod]
        public async Task CanSubmitDeploymentAndPollResultsAndDelete()
        {
            int jobId = await SubmitDeployment();
            int tries = 0;
            while (tries++ < 100)
            {
                var status = await PollForResults(jobId);
                if (status.status == "Succeeded")
                {
                    var result = JObject.Parse((string)status.result);
                    Assert.AreEqual("Success", result["status"].Value<string>());
                    int deleteJobId = await DeleteDeployment((string)status.result);
                    int deleteTries = 0;

                    while (deleteTries++ < 100)
                    {
                        var deleteStatus = await PollForResults(deleteJobId);
                        if (deleteStatus.status == "Succeeded")
                        {
                            Assert.AreEqual("\"Success\"", (string)deleteStatus.result);
                            return;
                        }
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }

                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            Assert.Fail("Deployment timed out");
        }



        private async Task<int> SubmitDeployment()
        {
            using (var sr = new StreamReader("VIPTemplateMessage.json"))
            using (var client = new HttpClient())
            {
                var result = await client.PostAsync("http://localhost:60717/api/Deployment", new StringContent(sr.ReadToEnd(), Encoding.UTF8, "application/json"));
                result.EnsureSuccessStatusCode();
                var resultContent = await result.Content.ReadAsStringAsync();
                int jobId;
                bool parsed = Int32.TryParse(resultContent.Replace("\"", ""), out jobId);
                Assert.IsTrue(parsed);
                return jobId;
            }
        }


        private async Task<JobStatus> PollForResults(int jobId)
        {
            using (var client = new HttpClient())
            {
                var resultContent = await client.GetStringAsync($"http://localhost:60717/api/Deployment/{jobId}");
                var status = JsonConvert.DeserializeObject<JobStatus>(resultContent);
                Assert.IsTrue(status.status == "Enqueued" || status.status == "Processing" || status.status == "Succeeded");
                return status;
            }
        }

        private async Task<int> DeleteDeployment(string deploymentLog)
        {
            using (var sr = new StreamReader("VIPTemplateMessage.json"))
            using (var client = new HttpClient())
            {
                var document = new DeleteDocument();
                document.deploymentLog = JsonConvert.DeserializeObject<DeploymentLog>(deploymentLog);
                string deployDocString = sr.ReadToEnd();
                var deployDocument = JsonConvert.DeserializeObject<DeploymentDocument>(deployDocString);
                document.accountDetails = deployDocument.accountDetails;
                var jsonDoc = JsonConvert.SerializeObject(document);

                var request = new HttpRequestMessage(HttpMethod.Delete, "http://localhost:60717/api/Deployment");
                request.Content = new StringContent(jsonDoc, Encoding.UTF8, "application/json");
                var result = await client.SendAsync(request); 
                result.EnsureSuccessStatusCode();
                var resultContent = await result.Content.ReadAsStringAsync();
                int jobId;
                bool parsed = Int32.TryParse(resultContent.Replace("\"", ""), out jobId);
                Assert.IsTrue(parsed);
                return jobId;
            }
        }
    }
}
