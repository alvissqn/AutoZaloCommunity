using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using log4net;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.ViewModel
{
    public class ConsoleOutput : NotifyPropertyChanged
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ConsoleOutput));
        private ICommand _stopProcessCommand;
        private ICommand _pauseProcessCommand;
        private ICommand _playProcessCommand;
        private ICommand _closeConsoleWindowCommand;

        private Process _windowProcess;
        private bool _isPause;
        private bool _isStop;

        public ICommand TerminateProcessCommand => _stopProcessCommand ?? (_stopProcessCommand = new RelayCommand(Terminate));
        public ICommand SuspendProcessCommand => _pauseProcessCommand ?? (_pauseProcessCommand = new RelayCommand(Suspend));
        public ICommand ResumeProcessCommand => _playProcessCommand ?? (_playProcessCommand = new RelayCommand(Resume));
        public ICommand CloseConsoleWindowCommand => _closeConsoleWindowCommand ?? (_closeConsoleWindowCommand = new RelayCommand(CloseWindow));

        private void CloseWindow()
        {
            try
            {
                ServiceLocator.Current.GetInstance<ConsoleOutputViewModel>().ConsoleOutputs.Remove(this);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public ConsoleOutput(string account, string device, string type)
        {
            Account = account;
            Device = device;
            Type = type;
        }

        public string Account { get; }
        public string Device { get; }
        public string Type { get; }

        public bool IsSuspended
        {
            get { return _isPause; }
            set
            {
                _isPause = value;
                RaisePropertyChanged();
            }
        }

        public bool IsTerminated
        {
            get { return _isStop; }
            set
            {
                _isStop = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Outputs { get; set; } = new ObservableCollection<string>();

        public void Received(string data) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
        {
            if (data?.StartsWith("ZALO", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                Outputs.Add(data);
            }
        });

        public void SetWindowProcess(Process process)
        {
            _windowProcess = process;
            process.EnableRaisingEvents = true;
            _windowProcess.Exited += WindowProcess_Exited;
        }

        public void Terminate()
        {
            try
            {
                if (!IsTerminated)
                {
                    _windowProcess.Kill();
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsTerminated = true);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public void Suspend()
        {
            try
            {
                if (!IsTerminated && !IsSuspended)
                {
                    _windowProcess.Suspend();
                    IsSuspended = true;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public void Resume()
        {
            try
            {
                if (!IsTerminated && IsSuspended)
                {
                    _windowProcess.Resume();
                    IsSuspended = false;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        private void WindowProcess_Exited(object sender, EventArgs e)
        {
            Received("Đã đóng tiến trình.");
            DispatcherHelper.CheckBeginInvokeOnUI(() => IsTerminated = true);
        }
    }

    public class ConsoleOutputViewModel
    {
        public ObservableCollection<ConsoleOutput> ConsoleOutputs { get; } = new ObservableCollection<ConsoleOutput>();
    }

    public static class ProcessExtension
    {
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);

        public static void Suspend(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    break;
                }
                SuspendThread(pOpenThread);
            }
        }

        public static void Resume(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    break;
                }
                ResumeThread(pOpenThread);
            }
        }
    }
}