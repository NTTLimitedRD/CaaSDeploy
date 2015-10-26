using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library;
using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Utilities;

namespace DD.CBU.CaasDeploy
{
    /// <summary>
    /// A command line application wrapper for the template deployment library.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        static void Main(string[] args)
        {
            Dictionary<string, string> arguments = ParseArguments(args);
            if (!ValidateArguments(arguments))
            {
                ShowUsage();
                return;
            }

            var t = Task.Run(() => PerformRequest(arguments));
            t.Wait();
        }

        /// <summary>
        /// Parses the arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The parsed arguments.</returns>
        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            var arguments = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i += 2)
            {
                if (i + 1 < args.Length)
                {
                    arguments.Add(args[i].ToLower().Substring(1), args[i + 1]);
                }
            }
            return arguments;
        }

        /// <summary>
        /// Validates the arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>True if arguments are valid; otherwise false.</returns>
        private static bool ValidateArguments(Dictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey("action") || !new string[] { "deploy", "delete" }.Contains(arguments["action"].ToLower()))
            {
                return false;
            }

            if (arguments["action"].ToLower() == "deploy")
            {
                if (!arguments.ContainsKey("action") ||
                    !arguments.ContainsKey("template") ||
                    !arguments.ContainsKey("deploymentlog") ||
                    !arguments.ContainsKey("region") ||
                    !arguments.ContainsKey("username") ||
                    !arguments.ContainsKey("password"))
                {
                    return false;
                }
            }
            else if (arguments["action"].ToLower() == "delete")
            {
                if (!arguments.ContainsKey("action") ||
                    !arguments.ContainsKey("deploymentlog") ||
                    !arguments.ContainsKey("region") ||
                    !arguments.ContainsKey("username") ||
                    !arguments.ContainsKey("password"))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Writes the usage help to the console.
        /// </summary>
        private static void ShowUsage()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("\tCaasDeploy.exe");
            Console.WriteLine("\t\t-action Deploy|Delete");
            Console.WriteLine("\t\t[-template {PathToTemplateFile}]  (required for Deploy)");
            Console.WriteLine("\t\t[-parameters {PathToParametersFile}]  (optional for Deploy)");
            Console.WriteLine("\t\t-deploymentlog {PathToLogFile}");
            Console.WriteLine("\t\t-region {RegionName}");
            Console.WriteLine("\t\t-username {CaaSUserName}");
            Console.WriteLine("\t\t-password {CaasPassword}");
        }

        /// <summary>
        /// Performs the request.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        static async Task PerformRequest(Dictionary<string, string> arguments)
        {
            try
            {
                var accountDetails = await CaasAuthentication.Authenticate(
                    arguments["username"],
                    arguments["password"],
                    arguments["region"]);

                var logProvider = new ConsoleLogProvider();
                var parser = new TemplateParser(new ConsoleLogProvider());

                if (arguments["action"].ToLower() == "deploy")
                {
                    var parametersFile = arguments.ContainsKey("parameters") ? arguments["parameters"] : null;
                    var templateFile = arguments["template"];

                    var taskExecutor = parser.ParseDeploymentTemplate(accountDetails, templateFile, parametersFile);
                    var log = await taskExecutor.Execute();

                    Console.WriteLine($"Result: {log.Status}");

                    log.SaveToFile(arguments["deploymentlog"]);
                    Console.WriteLine($"Complete! Deployment log written to {arguments["deploymentlog"]}.");
                }
                else if (arguments["action"].ToLower() == "delete")
                {
                    var deploymentLogFile = arguments["deploymentlog"];

                    var taskExecutor = parser.ParseDeploymentLog(accountDetails, deploymentLogFile);
                    var log = await taskExecutor.Execute();

                    Console.WriteLine($"Result: {log.Status}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
