using log4net;
using Managed.Adb;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.ViewModel
{
    public class ZaloCommunityService
    {
        private ILog log = LogManager.GetLogger(nameof(ZaloCommunityService));

        string WorkingFolderPath = @"C:\Users\ngan\Desktop\ZaloCommunityDev\ZaloCommunityDev.Service\bin\Debug";

        string Filename = @"ZaloCommunityDev.Service.exe";

        AndroidDebugBridge _adb;
        public string AndroidDebugBridgeOsLocation { get; set; } = @"C:\Program Files\Leapdroid\VM";

        Settings settings;
        public ZaloCommunityService(Settings settings)
        {
            this.settings = settings;

            try
            {
                _adb = AndroidDebugBridge.CreateBridge(Path.Combine(AndroidDebugBridgeOsLocation, "adb.exe"), true);
                _adb.Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public string[] OnlineDevices => _adb?.Devices.Where(x => x.IsOnline).Select(x => x.SerialNumber).ToArray();

        public void KillProcess()
        {
            CurrentProcess.Close();
        }

        internal async Task AddFriendNearBy(Filter x, Action<string> textReceived, Action<string> completed)
        {
            string sessionText = Guid.NewGuid().ToString("N").ToLower();
            var newpath = Path.Combine(WorkingFolderPath, "WorkingSession", sessionText);
            Directory.CreateDirectory(newpath);

            var filterText =  JsonConvert.SerializeObject(x, Formatting.Indented);
            var settingsText = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(Path.Combine(newpath, "filter.json"), filterText);
            File.WriteAllText(Path.Combine(newpath, "setting.json"), settingsText);

            await Task.Factory.StartNew(() =>
            {
                RunZaloService("add-friend-near-by", textReceived, sessionText);
            });
        }

        internal async Task SpamFriend(Filter x)
        {
            //throw new NotImplementedException();
        }

        private Process CurrentProcess { get; set; }
        
        public string RunZaloService(string type, Action<string> textReceived, string arguments = null)
        {
            var process = new Process();
            CurrentProcess = process;

            process.StartInfo.FileName = Path.Combine(WorkingFolderPath, Filename);
            if (!string.IsNullOrEmpty(arguments))
            {
                process.StartInfo.Arguments = type + " " + arguments;
            }
            
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = WorkingFolderPath;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            var stdOutput = new StringBuilder();
            process.OutputDataReceived += (sender, args) => textReceived(args.Data);

            string stdError = null;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                stdError = process.StandardError.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                throw new Exception("OS error while executing " + Format(Filename, arguments) + ": " + e.Message, e);
            }

            if (process.ExitCode == 0)
            {
                return stdOutput.ToString();
            }
            else
            {
                var message = new StringBuilder();

                if (!string.IsNullOrEmpty(stdError))
                {
                    message.AppendLine(stdError);
                }

                if (stdOutput.Length != 0)
                {
                    message.AppendLine("Std output:");
                    message.AppendLine(stdOutput.ToString());
                }
                
                textReceived(Format(Filename, arguments) + " finished with exit code = " + process.ExitCode + ": " + message);

                return message.ToString();
            }
        }

        private string Format(string filename, string arguments)
        {
            return "'" + filename +
                ((string.IsNullOrEmpty(arguments)) ? string.Empty : " " + arguments) +
                "'";
        }
    }
}
