using System.Collections.Generic;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Macros;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DD.CBU.CaasDeploy.Library.Tests.Macros
{
    /// <summary>
    /// Unit tests for the <see cref="ParametersMacro"/> class.
    /// </summary>
    [TestClass]
    public class ParametersMacroTests
    {
        /// <summary>
        /// Tests the macro under normal conditions.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Parameters_SubstituteTokensInString_Success()
        {
            var context = new TaskContext
            {
                Parameters = new Dictionary<string, string>()
                {
                    { "Param1", "Value1" },
                    { "Param2", "Value2" },
                    { "Param3", "Value3" }
                }
            };

            var macro = new ParametersMacro();
            var input = "Hello_$parameters['Param2']";
            var output = await macro.SubstituteTokensInString(null, context, input);

            Assert.AreEqual("Hello_Value2", output);
        }

        /// <summary>
        /// Tests the macro under error conditions.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        [ExpectedException(typeof(TemplateParserException))]
        public async Task Parameters_SubstituteTokensInString_NotFound()
        {
            var context = new TaskContext
            {
                Parameters = new Dictionary<string, string>()
                {
                    { "Param1", "Value1" }
                }
            };

            var macro = new ParametersMacro();
            var input = "Hello_$parameters['Param2']";
            await macro.SubstituteTokensInString(null, context, input);
        }
    }
}
