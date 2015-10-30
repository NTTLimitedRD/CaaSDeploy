using System;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Macros;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DD.CBU.CaasDeploy.Library.Tests.Macros
{
    /// <summary>
    /// Unit tests for the <see cref="ImageMacro"/> class.
    /// </summary>
    [TestClass]
    public class ImageMacroTests
    {
        /// <summary>
        /// The org identifier.
        /// </summary>
        private const string OrgId = "68819F2B-22F9-4D46-86B8-7926B640464C";

        /// <summary>
        /// The runtime context.
        /// </summary>
        private RuntimeContext _runtimeContext = new RuntimeContext
        {
            LogProvider = new ConsoleLogProvider(),
            AccountDetails = new CaasAccountDetails
            {
                BaseUrl = "https://api-na.dimensiondata.com",
                OrgId = OrgId
            }
        };

        /// <summary>
        /// Tests the macro under normal conditions.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task ServerImage_SubstituteTokensInString_Success()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/oec/0.9/base/imageWithDiskSpeed?name=RedHat 6 64-bit 2 CPU", "Image_Get.xml");

            var macro = new ImageMacro();
            var input = "$serverImage['NA2', 'RedHat 6 64-bit 2 CPU']";
            var output = await macro.SubstituteTokensInString(_runtimeContext, null, input);

            Assert.AreEqual("0bf731a8-29c5-4b8b-a460-2a60ab4019cf", output);
        }

        /// <summary>
        /// Tests the macro under error conditions.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        [ExpectedException(typeof(TemplateParserException))]
        public async Task ServerImage_SubstituteTokensInString_NotFound()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/oec/0.9/base/imageWithDiskSpeed?name=RedHat 6 64-bit 2 CPU", "Image_Get_NotFound.xml");

            var macro = new ImageMacro();
            var input = "$serverImage['NA2', 'RedHat 6 64-bit 2 CPU']";
            await macro.SubstituteTokensInString(_runtimeContext, null, input);
        }

        /// <summary>
        /// Tests the macro under normal conditions.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task CustomerImage_SubstituteTokensInString_Success()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/oec/0.9/" + OrgId + "/imageWithDiskSpeed?name=RedHat 6 64-bit 2 CPU", "Image_Get.xml");

            var macro = new ImageMacro();
            var input = "$customerImage['NA2', 'RedHat 6 64-bit 2 CPU']";
            var output = await macro.SubstituteTokensInString(_runtimeContext, null, input);

            Assert.AreEqual("0bf731a8-29c5-4b8b-a460-2a60ab4019cf", output);
        }

        /// <summary>
        /// Tests the macro under error conditions.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        [ExpectedException(typeof(TemplateParserException))]
        public async Task CustomerImage_SubstituteTokensInString_NotFound()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/oec/0.9/" + OrgId + "/imageWithDiskSpeed?name=RedHat 6 64-bit 2 CPU", "Image_Get_NotFound.xml");

            var macro = new ImageMacro();
            var input = "$customerImage['NA2', 'RedHat 6 64-bit 2 CPU']";
            await macro.SubstituteTokensInString(_runtimeContext, null, input);
        }
    }
}
