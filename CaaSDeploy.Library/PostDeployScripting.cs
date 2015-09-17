using CaasDeploy.Library.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    public class PostDeployScripting
    {
        public enum OSType
        {
            Windows,
            Linux,
        }

        private OSType _osType;
        private string _serverIP;
        private string _userName;
        private string _password;

        private bool _serverReady;

        public PostDeployScripting(string serverIP, string userName, string password, OSType osType)
        {
            _serverIP = serverIP;
            _osType = osType;
            _userName = userName;
            _password = password;
        }

        private void WaitForServerToBeReady()
        {
            if (_serverReady)
            {
                return;
            }
            _serverReady = true;
        }


        public void UploadScript(string localPath)
        {
            WaitForServerToBeReady();

            if (_osType == OSType.Windows)
            {
                UploadWindowsScript(localPath);
            }
            else if (_osType == OSType.Linux)
            {
                UploadLinuxScript(localPath);
            }
        }



        private void UploadWindowsScript(string localPath)
        {
            var creds = new NetworkCredential(_userName, _password);
            using (new NetworkConnection(@"\\" + _serverIP + @"\admin$", creds))
            {
                File.Copy(localPath, @"\\" + _serverIP + @"\admin$\" + Path.GetFileName(localPath), true);
            }
        }

        private void UploadLinuxScript(string localPath)
        {
            throw new NotImplementedException();
        }

        public int ExecuteScript(string commandLine)
        {
            WaitForServerToBeReady();

            if (_osType == OSType.Windows)
            {
                return ExecuteWindowsScript(commandLine);
            }
            else if (_osType == OSType.Linux)
            {
                return ExecuteLinuxScript(commandLine);
            }
            throw new InvalidOperationException();
        }

        private int ExecuteWindowsScript(string commandLine)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "psexec.exe",
                Arguments = $"\\\\{_serverIP} -u {_userName} -p {_password} cmd.exe /c " + commandLine,  
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();
            return process.ExitCode;
        }

        private int ExecuteLinuxScript(string commandLine)
        {
            throw new NotImplementedException();
        }
    }
}
