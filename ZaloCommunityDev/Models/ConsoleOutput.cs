using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using log4net;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.ViewModel;

namespace ZaloCommunityDev.Models
{
    public class ConsoleOutput : NotifyPropertyChanged
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ConsoleOutput));
        private ICommand _stopProcessCommand;
        private ICommand _pauseProcessCommand;
        private ICommand _playProcessCommand;
        private ICommand _closeConsoleWindowCommand;

        private Process _windowProcess;
        private bool _isSuspended;
        private bool _isTerminated;

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
            get { return _isSuspended; }
            set
            {
                _isSuspended = value;
                RaisePropertyChanged();
            }
        }

        public bool IsTerminated
        {
            get { return _isTerminated; }
            set
            {
                _isTerminated = value;
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
}