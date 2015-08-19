using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    internal static class TemplateParser
    {
        public static DeploymentTemplate ParseTemplate(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var content = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<DeploymentTemplate>(content);
            }
        }

        public static Dictionary<string, string> ParseParameters(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var dict = new Dictionary<string, string>();
                var content = reader.ReadToEnd();
                var jObject = JObject.Parse(content);
                foreach (var param in ((JObject)jObject["parameters"]).Properties())
                {
                    dict.Add(param.Name, param.Value["value"].Value<string>());
                }
                return dict;
            }
        }

        public static DeploymentLog ParseDeploymentLog(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var content = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<DeploymentLog>(content);
            }
        }
    }
}
