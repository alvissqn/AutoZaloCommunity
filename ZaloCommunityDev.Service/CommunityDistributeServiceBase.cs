﻿using log4net;
using Managed.Adb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Service.Models;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Service
{
    public abstract class CommunityDistributeServiceBase
    {
        public static string WorkingPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private readonly ILog _log = LogManager.GetLogger(nameof(CommunityDistributeServiceBase));

        protected readonly DatabaseContext DbContext;
        protected readonly Settings Settings;
        protected readonly IZaloImageProcessing ZaloImageProcessing;

        protected readonly ZaloAdbRequest ZaloAdbRequest;

        protected bool IsDebug => Settings.IsDebug;
        protected ScreenInfo Screen => Settings.Screen;

        protected CommunityDistributeServiceBase(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest ZaloAdbRequest)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(settings.AndroidDebugBridgeOsWorkingLocation))
                throw new ArgumentException("Must declare AndroidDebugBridge Os Location");

            ZaloImageProcessing = zaloImageProcessing;

            Settings = settings;

            DbContext = dbContext;

            this.ZaloAdbRequest = ZaloAdbRequest;
        }

        public string CaptureScreenNow()
        {
            var fileName = DateTime.Now.Ticks.ToString();
            InvokeProc($"/c adb shell screencap -p sdcard/zalocommunitydev/_screen.png");

            Delay(100);

            InvokeProc($"/c adb pull sdcard/zalocommunitydev/_screen.png \"{ConvertToAndroidPath(WorkingPath)}/ImageData/CaptureScreen/{fileName}.png\"");

            Delay(100);

            return $@"{WorkingPath}\ImageData\CaptureScreen\{fileName}.png";
        }

        public void ChangeDisplayDensity(int des)
        {
            if (ZaloAdbRequest.Device == null || ZaloAdbRequest.Device.State == DeviceState.Online)
            {
                _log.Error("Device null");
                return;
            }

            ZaloAdbRequest.Device.ExecuteShellCommand("adb root", ZaloAdbRequest.ConsoleOutputReceiver);
            ZaloAdbRequest.Device.ExecuteShellCommand($"adb shell am display-density {des}", ZaloAdbRequest.ConsoleOutputReceiver);
        }

        public void ChangeDisplaySize(int x, int y)
        {
            if (ZaloAdbRequest.Device == null || ZaloAdbRequest.Device.State == DeviceState.Online)
            {
                _log.Error("Device null");
                return;
            }

            ZaloAdbRequest.Device.ExecuteShellCommand("adb root", ZaloAdbRequest.ConsoleOutputReceiver);
            ZaloAdbRequest.Device.ExecuteShellCommand($@"adb shell am display-size {x}x{y}", ZaloAdbRequest.ConsoleOutputReceiver);
        }

        public void Delay(int milisecond) => Thread.Sleep(milisecond);

        public void EnableAbdKeyoard()
        {
            InvokeProc("/c adb shell ime set com.android.adbkeyboard/.AdbIME");
            Thread.Sleep(500);
        }

        public void SendKey(KeyCode keycode, int times = 1)
        {
            //ZaloHelper.Output("Debug: SendKey " + keycode.ToString() + ". Time : " + times);
            if ((ZaloAdbRequest.Device != null) && (ZaloAdbRequest.Device.State == DeviceState.Online))
            {
                for (var i = 0; i < times; i++)
                {
                    ZaloAdbRequest.Device.ExecuteShellCommand("input keyevent " + (int)keycode, ZaloAdbRequest.ConsoleOutputReceiver);
                    Delay(Settings.Delay.PressedKeyEvent);
                }
            }
            else
            {
                _log.Error("Can't send keyevent");
            }
        }

        public void SendText(string text)
        {
            if (ZaloAdbRequest.Device == null || ZaloAdbRequest.Device.State != DeviceState.Online)
            {
                _log.Error("Device null");

                return;
            }

            if (Encoding.UTF8.GetBytes(text).Length <= 0x3d0)
            {
                ZaloAdbRequest.Device.ExecuteShellCommand("am broadcast -a ADB_INPUT_TEXT --es msg '" + text + "'", ZaloAdbRequest.ConsoleOutputReceiver);
            }
            else
            {
                foreach (var buffer2 in SplitIntoChunks(Encoding.UTF8.GetBytes(text), 0x3d0))
                {
                    ZaloAdbRequest.Device.ExecuteShellCommand("am broadcast -a ADB_INPUT_TEXT --es msg '" + Encoding.UTF8.GetString(buffer2) + "'", ZaloAdbRequest.ConsoleOutputReceiver);
                }
            }

            Delay(Settings.Delay.PressedKeyEvent);
        }

        /// <summary>
        /// $ telnet localhost 5554
        ///Android Console: type 'help' for a list of commands
        ///OK
        ///geo fix -82.411629 28.054553
        ///OK
        /// </summary>
        /// <param name="areaNameOrCoord"></param>
        /// <returns></returns>
        public string SetLocationByName(string areaNameOrCoord)
        {
            //"Enable Telnet";
            //dism /online /Enable-Feature /FeatureName:TelnetClient

            if (string.IsNullOrWhiteSpace(areaNameOrCoord))
                return string.Empty;

            SetGpsByCxdeberryGeoTag(areaNameOrCoord);

            try
            {
                string locationText = null;
                Action<string> locationCallBack = x => locationText = x;
                InvokeProc("/c adb shell dumpsys location", locationCallBack);

                var logLat = locationText?.Split("\r\n".ToArray())?.LastOrDefault(x => x.TrimStart().StartsWith("Location[gps"))?.Substring("gps ", " acc");

                return $"[{logLat}] / ({areaNameOrCoord})";
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return string.Empty;
            }
        }

        public void SetGpsByNewapphorizons(string areaNameOrCoord)
        {
            try
            {
                InvokeProc("/c adb shell am force-stop com.blogspot.newapphorizons.fakegps");

                Delay(Settings.Delay.CloseMap);

                InvokeProc("/c adb shell am start -n com.blogspot.newapphorizons.fakegps/com.blogspot.newapphorizons.fakegps.MainActivity");

                Delay(Settings.Delay.OpenMap);

                TouchAt(Screen.FakeGpsScreenMenu);

                Delay(500);

                TouchAt(Screen.FakeGpsSearch);

                Delay(500);

                TouchAt(Screen.FakeGpsLocationTab);

                Delay(500);

                TouchAt(Screen.FakeGpsCountryTextField);

                SendText(areaNameOrCoord);

                Delay(500);

                TouchAt(Screen.FakeGpsLocationOkButton);

                TouchAt(Screen.FakeGpsStopButton);
                Delay(500);

                TouchAt(Screen.FakeGpsStartButton);
                Delay(300);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public void SetGpsByCxdeberryGeoTag(string areaNameOrCoord)
        {
            try
            {
                InvokeProc("/c adb shell am force-stop com.cxdeberry.geotag");

                Delay(Settings.Delay.CloseMap);

                InvokeProc("/c adb shell am start -n com.cxdeberry.geotag/.MainActivity");

                Delay(Settings.Delay.OpenMap);

                TouchAt(Screen.IconTopRight);
                Delay(400);

                SendText(areaNameOrCoord);
                SendKey(KeyCode.AkeycodeEnter);
                Delay(2000);
                TouchAt(Screen.FakeGps2StartButton);

                Delay(Settings.Delay.CloseMap);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public void SetKeyBoardTelex(bool check)
        {
            if (ZaloAdbRequest.Device == null || ZaloAdbRequest.Device.State == DeviceState.Online)
            {
                _log.Error("Device null");
                return;
            }

            ZaloAdbRequest.Device.ExecuteShellCommand("adb shell ime set com.android.adbkeyboard/.AdbIME", ZaloAdbRequest.ConsoleOutputReceiver);
            Thread.Sleep(500);
        }

        public void DelImagePost()
        {
            try
            {
                InvokeProc("/c adb shell rm -f sdcard/DCIM/*.*");
                Thread.Sleep(100);
            }
            catch
            {
            }
            Thread.Sleep(0x3e8);
        }

        private static string ConvertToAndroidPath(string path) => path.Replace(@"\", @"/");

        public void UpImage(FileInfo path)
        {
            InvokeProc($"/c adb push \"{ConvertToAndroidPath(path.FullName)}\" sdcard/DCIM && adb shell am broadcast -a android.intent.action.MEDIA_MOUNTED -d file:///sdcard/DCIM/");
            Thread.Sleep(100);
        }

        public void UpImageChat(FileInfo path)
        {
            InvokeProc("/c adb push \"" + ConvertToAndroidPath(path.FullName) + "\" sdcard/DCIM/chat.png && adb shell am broadcast -a android.intent.action.MEDIA_MOUNTED -d file:///sdcard/DCIM/");
            Thread.Sleep(100);
        }

        public void UpImagePost(FileInfo path)
        {
            InvokeProc("/c adb push \"" + ConvertToAndroidPath(path.FullName) + "\" sdcard/DCIM && adb shell am broadcast -a android.intent.action.MEDIA_MOUNTED -d file:///sdcard/DCIM/");
            Thread.Sleep(100);
        }

        protected void DeleteWordInFocusedTextField(int word = 15) => SendKey(KeyCode.AkeycodeDel, word);

        protected ProfileMessage GrabProfileInfo(string initName = null)
        {
            ZaloHelper.Output($"!đang lấy thông tin bạn: {initName}");

            TouchAt(Screen.ProfileScreenTabInfo); //Touch tab Thong Tin
            Delay(300);

            var file = CaptureScreenNow();
            var profile = ZaloImageProcessing.GetProfile(file, Screen);
            if (!string.IsNullOrWhiteSpace(initName))
            {
                profile.Name = initName;
            }
            ZaloHelper.Output($"!@: {profile.Name} {profile.BirthdayText} {profile.Gender} {profile.PhoneNumber}");
            return profile;
        }

        protected void InvokeProc(string args, Action<string> output=null)
        {
            var hasOutput = output != null;
            var process = new Process
            {
                StartInfo = {
                               UseShellExecute = !hasOutput,
                               WorkingDirectory = ZaloAdbRequest.AdbPath,
                               FileName = @"C:\Windows\System32\cmd.exe",
                               Arguments = args,
                               WindowStyle = ProcessWindowStyle.Hidden,
                               RedirectStandardOutput = hasOutput,
                               RedirectStandardError = hasOutput
                            }
            };

            var builder = new StringBuilder();
            process.Start();
            process.WaitForExit();
            if (hasOutput)
            {
                var outputText = process.StandardOutput.ReadToEnd();
                var errorText = process.StandardError.ReadToEnd();
                if(!string.IsNullOrWhiteSpace(errorText))
                {
                    output(errorText);
                }
                else
                {
                    output(outputText);
                }
            }
        }

        protected void ScrollList(int times)
            => TouchSwipe(Screen.WorkingRect.Center.X, Screen.WorkingRect.Center.Y, Screen.WorkingRect.Center.X, Screen.WorkingRect.Center.Y - Screen.FriendRowHeight, times);

        private static string Spintext(Random rnd, string str)
        {
            var pattern = "{[^{}]*}";
            for (var match = Regex.Match(str, pattern); match.Success; match = Regex.Match(str, pattern))
            {
                var strArray = str.Substring(match.Index + 1, match.Length - 2).Split(new char[] { '|' });
                str = str.Substring(0, match.Index) + strArray[rnd.Next(strArray.Length)] + str.Substring(match.Index + match.Length);
            }
            return str;
        }

        private IEnumerable<byte[]> SplitIntoChunks(byte[] value, int bufferLength)
        {
            var iteratorVariable0 = value.Length / bufferLength;
            if ((value.Length % bufferLength) > 0)
            {
                iteratorVariable0++;
            }
            for (var i = 0; i < iteratorVariable0; i++)
            {
                yield return value.Skip<byte>((i * bufferLength)).Take<byte>(bufferLength).ToArray<byte>();
            }
        }

        #region Touches

        public void TouchAt(ScreenPoint point, int times = 1)
            => TouchAt(point.X, point.Y, times);

        public void TouchAt(int x, int y, int times = 1)
        {
            if ((ZaloAdbRequest.Device != null) && (ZaloAdbRequest.Device.State == DeviceState.Online))
            {
                for (var i = 0; i < times; i++)
                {
                    ZaloAdbRequest.Device.ExecuteShellCommand($"input tap {x} {y}", ZaloAdbRequest.ConsoleOutputReceiver);
                    Delay(Settings.Delay.TouchEvent);
                }
            }
            else
            {
                _log.Error("Can't touch");
            }
        }

        public void TouchAtIconBottomLeft()
            => TouchAt(Screen.IconBottomLeft);

        public void TouchAtIconBottomRight()
            => TouchAt(Screen.IconBottomRight);

        public void TouchAtIconTopLeft()
            => TouchAt(Screen.IconTopLeft);

        public void TouchAtIconTopRight()
            => TouchAt(Screen.IconTopRight);

        public void TouchSwipe(ScreenPoint point1, ScreenPoint point2, int times = 1)
            => TouchSwipe(point1.X, point1.Y, point2.X, point2.Y, times);

        public void TouchSwipe(int x1, int y1, int x2, int y2, int times = 1)
        {
            if ((ZaloAdbRequest.Device != null) && (ZaloAdbRequest.Device.State == DeviceState.Online))
            {
                for (var i = 0; i < times; i++)
                {
                    InvokeProc($"/c adb shell input swipe {x1} {y1} {x2} {y2}");
                    Delay(Settings.Delay.ScrollEvent);
                }
            }
            else
            {
                _log.Error("Can't touch swipe");
            }
        }

        #endregion Touches
    }
}