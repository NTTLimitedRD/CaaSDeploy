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

            try
            {
                var d = new Deployment("TestTemplate.json", "TestTemplate.params.json", "NA", accountDetails);
                await d.Deploy();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
