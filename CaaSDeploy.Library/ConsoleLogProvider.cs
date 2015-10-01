using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    public class ConsoleLogProvider : ILogProvider
    {
        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        public void LogException(Exception exception)
        {
            LogError(exception.ToString());
        }

        public void LogMessage(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public void IncrementProgress()
        {
            Console.ResetColor();
            Console.Write(".");
        }

        public void CompleteProgress()
        {
            Console.ResetColor();
            Console.WriteLine("Done!");
        }
    }
}
