using System.Diagnostics;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.ViewModel;

namespace ZaloCommunityDev
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var consoleList = ServiceLocator.Current.GetInstance<ConsoleOutputViewModel>().ConsoleOutputs;
            foreach (var console in consoleList)
            {
                console.Terminate();
            }

            Process.GetCurrentProcess().Kill();
        }
    }
}