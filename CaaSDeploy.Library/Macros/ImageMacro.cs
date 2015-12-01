using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Utilities;

namespace DD.CBU.CaasDeploy.Library.Macros
{
    /// <summary>
    /// The server image marco resolves server image identifiers by name.
    /// </summary>
    public sealed class ImageMacro : IMacro
    {
        /// <summary>
        /// The server image base URL
        /// </summary>
        private const string ImageUrl = "/oec/0.9/{0}/imageWithDiskSpeed?name={1}";

        /// <summary>
        /// The server image regex
        /// </summary>
        private static readonly Regex ImageRegex = new Regex(@"\$(serverImage|customerImage)\['([^']*)'\s*,\s*'([^']*)'\]", RegexOptions.IgnoreCase);

        /// <summary>
        /// The CaaS XML namespace.
        /// </summary>
        private static readonly XNamespace ServerNamespace = "http://oec.api.opsource.net/schemas/server";

        /// <summary>
        /// Substitutes the property tokens in the supplied string.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <param name="input">The input string.</param>
        /// <returns>The substituted string</returns>
        public async Task<string> SubstituteTokensInString(RuntimeContext runtimeContext, TaskContext taskContext, string input)
        {
            string output = input;

            var matches = ImageRegex.Matches(input);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string imageType = match.Groups[1].Value;
                    string location = match.Groups[2].Value;
                    string imageName = match.Groups[3].Value;

                    using (var client = HttpClientFactory.GetClient(runtimeContext.AccountDetails, "text/xml"))
                    {
                        var url = (imageType == "customerImage")
                            ? string.Format(ImageUrl, runtimeContext.AccountDetails.OrgId, imageName)
                            : string.Format(ImageUrl, "base", imageName);

                        var response = await client.GetAsync(runtimeContext.AccountDetails.BaseUrl + url);
                        response.ThrowForHttpFailure();

                        var responseBody = await response.Content.ReadAsStringAsync();
                        var document = XDocument.Parse(responseBody);
                        var imageId = document.Root
                            .Elements(ServerNamespace + "image")
                            .Where(e => e.Attribute("location").Value == location)
                            .Select(e => e.Attribute("id").Value)
                            .FirstOrDefault();

                        if (imageId == null)
                        {
                            throw new TemplateParserException($"Image '{imageName}' not found in datacenter '{location}'.");
                        }

                        output = output.Replace(match.Groups[0].Value, imageId);
                    }
                }
            }

            return output;
        }
    }
}
