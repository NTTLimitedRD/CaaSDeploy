using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    public interface ILogProvider
    {
        void LogMessage(string message);

        void LogError(string message);

        void LogException(Exception exception);

        void IncrementProgress();
        void CompleteProgress();
    }
}
