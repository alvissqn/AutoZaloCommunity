using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.ViewModel
{
    public class ConsoleOutputViewModel
    {
        public ObservableCollection<ConsoleOutput> ConsoleOutputs { get; } = new ObservableCollection<ConsoleOutput>();
    }

    public class ConsoleOutput : NotifyPropertyChanged
    {
        private ICommand _stopProcessCommand;

        private Process _windowProcess;

        public ConsoleOutput(string account, string device, string type)
        {
            Account = account;
            Device = device;
            Type = type;
        }

        public string Type { get; }
        public string Account { get; }
        public string Device { get; }
        public ObservableCollection<string> Outputs { get; set; } = new ObservableCollection<string>();
        public ICommand StopProcessCommand => _stopProcessCommand ?? (_stopProcessCommand = new RelayCommand(() => _windowProcess.Close()));

        public void Received(string data) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
        {
            if (data?.StartsWith("ZALO") ?? false)
            {
                Outputs.Add(data);
            }

        });

        public void SetWindowProcess(Process process)
        {
            _windowProcess = process;
            _windowProcess.Exited += WindowProcess_Exited;
        }

        private void WindowProcess_Exited(object sender, EventArgs e)
        {
            Received("Đã đóng tiến trình.");
        }
    }
}