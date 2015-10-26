using System.IO;

using DD.CBU.CaasDeploy.Library.Models;
using Newtonsoft.Json;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
    /// <summary>
    /// Provides commonly used extension methods for the <see cref="DeploymentLog"/> class.
    /// </summary>
    public static class DeploymentTemplateLogExtensions
    {
        /// <summary>
        /// Saves the deployment log.
        /// </summary>
        /// <param name="log">The log to save.</param>
        /// <param name="fileName">Path to the file.</param>
        public static void SaveToFile(this DeploymentLog log, string fileName)
        {
            using (var sw = new StreamWriter(fileName))
            {
                var json = JsonConvert.SerializeObject(log, Formatting.Indented);
                sw.Write(json);
            }
        }
    }
}
