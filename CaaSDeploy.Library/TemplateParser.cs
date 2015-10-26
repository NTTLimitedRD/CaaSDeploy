using System.Collections.Generic;
using System.IO;

using DD.CBU.CaasDeploy.Library.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library
{
    /// <summary>
    /// Parses JSON deployment documents.
    /// </summary>
    public static class TemplateParser
    {
        /// <summary>
        /// Parses the template.
        /// </summary>
        /// <param name="fileName">Path to the file.</param>
        /// <returns>The parsed deployment template.</returns>
        public static DeploymentTemplate ParseTemplate(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var content = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<DeploymentTemplate>(content);
            }
        }

        /// <summary>
        /// Parses the parameters.
        /// </summary>
        /// <param name="fileName">Path to the file.</param>
        /// <returns>The parsed parameters.</returns>
        public static Dictionary<string, string> ParseParameters(string fileName)
        {
            var dict = new Dictionary<string, string>();
            if (fileName == null)
            {
                return dict;
            }

            using (var reader = new StreamReader(fileName))
            {
                var content = reader.ReadToEnd();
                var jObject = JObject.Parse(content);
                foreach (var param in ((JObject)jObject["parameters"]).Properties())
                {
                    dict.Add(param.Name, param.Value["value"].Value<string>());
                }
                return dict;
            }
        }

        /// <summary>
        /// Parses a deployment log file.
        /// </summary>
        /// <param name="fileName">Path to the file.</param>
        /// <returns>The parsed deployment log.</returns>
        public static DeploymentLog ParseDeploymentLog(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var content = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<DeploymentLog>(content);
            }
        }

        /// <summary>
        /// Saves the deployment log.
        /// </summary>
        /// <param name="log">The log to save.</param>
        /// <param name="fileName">Path to the file.</param>
        public static void SaveDeploymentLog(DeploymentLog log, string fileName)
        {
            using (var sw = new StreamWriter(fileName))
            {
                var json = JsonConvert.SerializeObject(log, Formatting.Indented);
                sw.Write(json);
            }
        }
    }
}
