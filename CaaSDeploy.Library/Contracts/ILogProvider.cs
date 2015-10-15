using System;

namespace CaasDeploy.Library.Contracts
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
