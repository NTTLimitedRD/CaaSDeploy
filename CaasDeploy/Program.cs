using CaasDeploy.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy
{
    class Program
    {

        static void Main(string[] args)
        {
            var t = Task.Run(() => Deploy());
            t.Wait();
        }

        static async Task Deploy()
        {
            var sr = new StreamReader(@"C:\temp\CaasCredentials.txt");
            string userName = sr.ReadLine();
            string password = sr.ReadLine();
            sr.Close();
            var accountDetails = await CaasAuthentication.Authenticate(userName, password);

            var d = new Deployment("TestTemplate.json", "TestTemplate.params.json", "NA", accountDetails);
            await d.Deploy();
        }
        static async Task CreateResources()
        {
            Console.WriteLine("Authenticating...");

            var sr = new StreamReader(@"C:\temp\CaasCredentials.txt");
            string userName = sr.ReadLine();
            string password = sr.ReadLine();
            sr.Close();

            var accountDetails = await CaasAuthentication.Authenticate(userName, password);

            Console.Write("Creating Network Domain...");
            var networkDomainJson = @"{
""datacenterId"": ""NA9"",
""name"": ""Toms Test Network Domain"",
""description"": ""Testing CaaS Deployment Templates"",
""type"": ""ESSENTIALS""
}";
            var networkDeployer = new ResourceDeployer("NetworkDomain", "NA", accountDetails);
            var networkProps = await networkDeployer.DeployAndWait(networkDomainJson);
            Console.WriteLine(" Done! ID: " + networkProps["id"]);

            Console.Write("Creating VLAN...");
            var vlanJson = String.Format(@"{{
""networkDomainId"": ""{0}"",
""name"": ""Toms Test VLAN"",
""description"": ""Testing CaaS Deployment Templates"",
""privateIpv4BaseAddress"": ""10.0.3.0""
}}", networkProps["id"]);

            var vlanDeployer = new ResourceDeployer("VLAN", "NA", accountDetails);
            var vlanProps = await vlanDeployer.DeployAndWait(vlanJson);
            Console.WriteLine(" Done! ID: " + vlanProps["id"]);

            Console.WriteLine("Creating Server...");
            var vmJson = String.Format(@"{{
        ""name"": ""Toms Test VM"",
        ""description"": ""Testing CaaS Deployment Templates"",
        ""imageId"": ""8bc629a9-8d71-4b1b-8b26-acdc077edab1"",
        ""start"": true,
        ""administratorPassword"": ""Password@1"",
        ""networkInfo"": {{
                    ""networkDomainId"": ""{0}"",
          ""primaryNic"": {{ ""vlanId"": ""{1}"" }},
          ""additionalNic"": []
        }},
        ""disk"": [
          {{
            ""scsiId"": ""0"",
            ""speed"": ""STANDARD""
          }}
        ]
      }}", networkProps["id"], vlanProps["id"]);
            var vmDeployer = new ResourceDeployer("Server", "NA", accountDetails);
            var vmProps = await vmDeployer.DeployAndWait(vmJson);
            Console.WriteLine(" Done! ID: " + vmProps["id"]);

            Console.WriteLine("Cleaning up...");
            await vmDeployer.DeleteAndWait(vmProps["id"]);
            Console.WriteLine("Deleted Server.");
            await vlanDeployer.DeleteAndWait(vlanProps["id"]);
            Console.WriteLine("Deleted VLAN.");
            await networkDeployer.DeleteAndWait(networkProps["id"]);
            Console.WriteLine("Deleted Network Domain.");
        }
    }
}
