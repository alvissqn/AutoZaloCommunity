using log4net;
using Managed.Adb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Service.Models
{
    public class ZaloAdbRequest
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloAdbRequest));

        public ZaloAdbRequest(Settings settings)
        {
            _receiver = new ConsoleOutputReceiver();

            AdbPath = settings.AndroidDebugBridgeOsLocation;

            try
            {
                Adb = AndroidDebugBridge.CreateBridge(Path.Combine(settings.AndroidDebugBridgeOsLocation, "adb.exe"), true);
                Adb.Start();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        protected AndroidDebugBridge Adb;
        public string AdbPath;

        private ConsoleOutputReceiver _receiver;

        private Device _device;

        public Device Device => _device;

        public ConsoleOutputReceiver ConsoleOutputReceiver => _receiver;

        public bool StartAvd(string serialNumberOrIndex)
        {
            if (Adb == null)
                return false;

            try
            {
                var value = 0;
                if (int.TryParse(serialNumberOrIndex, out value))
                {
                    _device = Adb.Devices.FirstOrDefault(x => x.IsOnline);
                }
                else
                {
                    _device = Adb.Devices.First(x => x.SerialNumber == serialNumberOrIndex && x.IsOnline);
                }

                return _device != null;
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                return false;
            }
        }

        public void StopAdb() => Adb.Stop();
    }
}
