using System.Collections.ObjectModel;
using ZaloCommunityDev.Models;

namespace ZaloCommunityDev.ViewModel
{
    public class ConsoleOutputViewModel
    {
        public ConsoleOutputViewModel()
        {
            ConsoleOutputs = new ObservableCollection<ConsoleOutput>();
        }

        public ObservableCollection<ConsoleOutput> ConsoleOutputs { get; }
    }
}