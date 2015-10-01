using CaasDeploy.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.PowerShell
{
    class PowerShellLogProvider : ILogProvider
    {
        PSCmdlet _cmdlet;

        public PowerShellLogProvider(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }
        public void CompleteProgress()
        {
            throw new NotImplementedException();
        }

        public void IncrementProgress()
        {



        }

        public void LogError(string message)
        {
            _cmdlet.WriteWarning(message);
        }

        public void LogException(Exception exception)
        {
            _cmdlet.WriteError(new ErrorRecord(exception, String.Empty, ErrorCategory.NotSpecified, null));
        }

        public void LogMessage(string message)
        {
            _cmdlet.WriteObject(message);
        }
    }
}
