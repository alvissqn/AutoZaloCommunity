using log4net;
using Managed.Adb;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZaloCommunityDev.Models;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.ViewModel
{
    public class ZaloCommunityService
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloCommunityService));

        //private const string WorkingFolderPath = @"C:\Users\diepnguyenv\Desktop\code\zalocommunitydev\ZaloCommunityDev.Service\bin\Debug";
        private const string WorkingFolderPath = @"C:\Users\ngan\Desktop\ZaloCommunityDev\ZaloCommunityDev.Service\bin\Debug";

        private const string Filename = @"ZaloCommunityDev.Service.exe";

        private readonly AndroidDebugBridge _adb;
        public string AndroidDebugBridgeOsLocation { get; set; } = @"C:\Program Files\Leapdroid\VM";

        private readonly Settings _settings;

        public ZaloCommunityService(Settings settings)
        {
            this._settings = settings;

            try
            {
                _adb = AndroidDebugBridge.CreateBridge(Path.Combine(AndroidDebugBridgeOsLocation, "adb.exe"), true);
                _adb.Start();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public string[] OnlineDevices => _adb?.Devices.Where(x => x.IsOnline).Select(x => x.SerialNumber).ToArray();

        public async Task AddFriendNearBy(Filter filter, ConsoleOutput consoleOutput)
        {
            var sessionId = CreateSession(filter);

            await Task.Factory.StartNew(() => { RunZaloService(RunnerConstants.addfriendnearby, sessionId, consoleOutput); });
        }

        public async Task AddFriendByPhone(Filter filter, ConsoleOutput consoleOutput)
        {
            var sessionId = CreateSession(filter);

            await Task.Factory.StartNew(() => { RunZaloService(RunnerConstants.addfriendbyphone, sessionId, consoleOutput); });
        }

        public async Task SendMessageToFriend(Filter filter, ConsoleOutput consoleOutput)
        {
            var sessionId = CreateSession(filter);

            await Task.Factory.StartNew(() => { RunZaloService(RunnerConstants.sendmessagetofriendsincontacts, sessionId, consoleOutput); });
        }

        public async Task SendMessageToStrangerByPhone(Filter filter, ConsoleOutput consoleOutput)
        {
            var sessionId = CreateSession(filter);

            await Task.Factory.StartNew(() => { RunZaloService(RunnerConstants.sendmessagebyphonenumber, sessionId, consoleOutput); });
        }

        public async Task SendMessageToStrangerNearBy(Filter filter, ConsoleOutput consoleOutput)
        {
            var sessionId = CreateSession(filter);

            await Task.Factory.StartNew(() => { RunZaloService(RunnerConstants.sendmessagenearby, sessionId, consoleOutput); });
        }

        public string RunZaloService(string type, string arguments, ConsoleOutput output)
        {
            try
            {
                arguments = (type + " " + arguments).Trim();
                var process = new Process
                {
                    StartInfo = {
                                FileName = Path.Combine(WorkingFolderPath, Filename),
                                Arguments = arguments,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                UseShellExecute = false,
                                WorkingDirectory = WorkingFolderPath,
                                RedirectStandardError = true,
                                RedirectStandardOutput = true ,
                                StandardOutputEncoding = Encoding.UTF8
                            }
                };

                output.SetWindowProcess(process);

                var stdOutput = new StringBuilder();

                process.OutputDataReceived += (sender, args) => output.Received(args.Data);

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
                    output.Received("OS error while executing " + Format(Filename, arguments) + ": " + e.Message);
                }

                if (process.ExitCode == 0)
                {
                    return stdOutput.ToString();
                }

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

                output.Received(Format(Filename, arguments) + " finished with exit code = " + process.ExitCode + ": " + message);

                return message.ToString();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return string.Empty;
            }
        }

        private string CreateSession(Filter x)
        {
            var sessionText = Guid.NewGuid().ToString("N").ToLower();
            var newpath = Path.Combine(WorkingFolderPath, "WorkingSession", sessionText);
            Directory.CreateDirectory(newpath);

            var filterText = JsonConvert.SerializeObject(x, Formatting.Indented);
            var settingsText = JsonConvert.SerializeObject(_settings, Formatting.Indented);
            File.WriteAllText(Path.Combine(newpath, "filter.json"), filterText);
            File.WriteAllText(Path.Combine(newpath, "setting.json"), settingsText);
            return sessionText;
        }

        private string Format(string filename, string arguments)
        {
            return "'" + filename +
                ((string.IsNullOrEmpty(arguments)) ? string.Empty : " " + arguments) +
                "'";
        }
    }
}