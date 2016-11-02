using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ConsoleOutputViewModel _consoleOutputViewModel;
        private readonly Settings _settings;
        private readonly ZaloCommunityService _zaloCommunityService;

        private User _currentUser;
        private string[] _onlineDevices;

        private string _selectedDevice;
        private ObservableCollection<User> _users;

        public MainViewModel(ZaloCommunityService service, DatabaseContext dbContext, Settings settings, ConsoleOutputViewModel consoleOutputViewModel)
        {
            _zaloCommunityService = service;
            _settings = settings;
            _consoleOutputViewModel = consoleOutputViewModel;

            Load();

            RefreshAvdListCommand = new RelayCommand(() => OnlineDevices = _zaloCommunityService.OnlineDevices);

            AutoAddFriendByPhoneCommand = new RelayCommand<Filter>(AutoAddFriendByPhoneInvoke);
            AutoAddFriendNearByCommand = new RelayCommand<Filter>(AutoAddFriendNearByInvoke);
            AutoSendMessageToFriendCommand = new RelayCommand<Filter>(AutoSendMessageToFriendInvoke);
            AutoSendMessageToStrangerByPhoneCommand = new RelayCommand<Filter>(AutoSendMessageToStrangerByPhoneInvoke);
            AutoSendMessageToStrangerNearByCommand = new RelayCommand<Filter>(AutoSendMessageToStrangerNearByInvoke);
        }

        public ICommand AutoAddFriendByPhoneCommand { get; }
        public ICommand AutoAddFriendNearByCommand { get; }
        public ICommand AutoSendMessageToFriendCommand { get; }
        public ICommand AutoSendMessageToStrangerByPhoneCommand { get; }
        public ICommand AutoSendMessageToStrangerNearByCommand { get; }
        public ICommand RefreshAvdListCommand { get; }

        #region Properties

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                RaisePropertyChanged();
            }
        }

        public string[] OnlineDevices
        {
            get { return _onlineDevices; }
            set { Set(ref _onlineDevices, value); }
        }

        public string SelectedDevice
        {
            get
            {
                return _selectedDevice;
            }
            set
            {
                Set(ref _selectedDevice, value);
            }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                RaisePropertyChanged();
            }
        }

        #endregion Properties

        public void Load()
        {
            OnlineDevices = _zaloCommunityService.OnlineDevices;
            Users = new ObservableCollection<User>(new[] { new User { Username = "0979864903", Password = "kimngan12345" } });

            CurrentUser = Users[0];
        }

        private ConsoleOutput CreateConsoleOutput(string type)
        {
            _settings.User = CurrentUser;
            _settings.DeviceNumber = SelectedDevice;
            var consoleOutput = new ConsoleOutput(_settings.User.Username, _settings.DeviceNumber, type);
            _consoleOutputViewModel.ConsoleOutputs.Add(consoleOutput);

            return consoleOutput;
        }

        private async void AutoAddFriendByPhoneInvoke(Filter filter)
        {
            var consoleOutput = CreateConsoleOutput("Kết bạn theo số đt");
            await _zaloCommunityService.AddFriendByPhone(filter, consoleOutput);
        }

        private async void AutoAddFriendNearByInvoke(Filter filter)
        {
            var consoleOutput = CreateConsoleOutput("Kết bạn theo vị trí");
            await _zaloCommunityService.AddFriendNearBy(filter, consoleOutput);
        }

        private async void AutoSendMessageToFriendInvoke(Filter filter)
        {
            var consoleOutput = CreateConsoleOutput("Gửi tin nhắn cho bạn");
            await _zaloCommunityService.SendMessageToFriend(filter, consoleOutput);
        }

        private async void AutoSendMessageToStrangerByPhoneInvoke(Filter filter)
        {
            var consoleOutput = CreateConsoleOutput("Gửi tin nhắn theo số đt");
            await _zaloCommunityService.SendMessageToStrangerByPhone(filter, consoleOutput);
        }

        private async void AutoSendMessageToStrangerNearByInvoke(Filter filter)
        {
            var consoleOutput = CreateConsoleOutput("Gửi tin nhắn theo vị trí");
            await _zaloCommunityService.SendMessageToStrangerNearBy(filter, consoleOutput);
        }
    }
}