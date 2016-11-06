using log4net;
using Managed.Adb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Service.Models
{
    public class ZaloAdbRequest
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloAdbRequest));

        public ZaloAdbRequest(Settings settings)
        {
            ConsoleOutputReceiver = new ConsoleOutputReceiver();

            AdbPath = settings.AndroidDebugBridgeOsWorkingLocation;

            try
            {
                Adb = AndroidDebugBridge.CreateBridge(Path.Combine(settings.AndroidDebugBridgeOsWorkingLocation, "adb.exe"), true);
                Adb.Start();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        protected AndroidDebugBridge Adb;
        public string AdbPath { get; }

        public Device Device { get; private set; }

        public ConsoleOutputReceiver ConsoleOutputReceiver { get; }

        public bool StartAvd(string serialNumberOrIndex)
        {
            if (Adb == null)
                return false;

            try
            {
                var value = 0;
                if (int.TryParse(serialNumberOrIndex, out value))
                {
                    Device = Adb.Devices.FirstOrDefault(x => x.IsOnline);
                }
                else
                {
                    Device = Adb.Devices.First(x => x.SerialNumber == serialNumberOrIndex && x.IsOnline);
                }

                return Device != null;
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
