﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using CaasDeploy.Library;
using CaasDeploy.Library.Contracts;

namespace CaasDeploy
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
            var config = (IComputeConfiguration)ConfigurationManager.GetSection("compute");

            var accountDetails = await CaasAuthentication.Authenticate(
                config,
                arguments["username"],
                arguments["password"],
                arguments["region"]);

            try
            {
                var taskBuilder = new TaskBuilder(new ConsoleLogProvider(), accountDetails);

                if (arguments["action"].ToLower() == "deploy")
                {
                    var parametersFile = arguments.ContainsKey("parameters") ? arguments["parameters"] : null;
                    var templateFile = arguments["template"];

                    var taskExecutor = taskBuilder.GetDeploymentTasks(templateFile, parametersFile);
                    var log = await taskExecutor.Execute();

                    Console.WriteLine($"Result: {log.status}");

                    TemplateParser.SaveDeploymentLog(log, arguments["deploymentlog"]);
                    Console.WriteLine($"Complete! Deployment log written to {arguments["deploymentlog"]}.");
                }
                else if (arguments["action"].ToLower() == "delete")
                {
                    var deploymentLogFile = arguments["deploymentlog"];

                    var taskExecutor = taskBuilder.GetDeletionTasks(deploymentLogFile);
                    var log = await taskExecutor.Execute();

                    Console.WriteLine($"Result: {log.status}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
