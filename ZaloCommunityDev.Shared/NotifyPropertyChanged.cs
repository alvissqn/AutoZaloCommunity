using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZaloCommunityDev.Shared
{
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => OnPropertyChanged(propertyName);
    }
}