using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Threading;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly ZaloCommunityService _zaloCommunityService;

        private ObservableCollection<Filter> _addFriendNearByConfigs;
        private ObservableCollection<Filter> _autoPostToFriendSessionConfigs;
        private string[] _onlineDevices;

        public ICommand AutoAddFriendCommand { get; }
        public ICommand AutoSpamFriendCommand { get; }

        public ICommand RefreshAvdListCommand { get; }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                RaisePropertyChanged();
            }
        }

        private readonly Settings _settings;
        private readonly ConsoleOutputViewModel _consoleOutputViewModel;

        public MainViewModel(ZaloCommunityService service, DatabaseContext dbContext, Settings settings, ConsoleOutputViewModel consoleOutputViewModel)
        {
            _zaloCommunityService = service;
            _databaseContext = dbContext;
            this._settings = settings;
            _consoleOutputViewModel = consoleOutputViewModel;

            Load();

            RefreshAvdListCommand = new RelayCommand(() => OnlineDevices = _zaloCommunityService.OnlineDevices);

            AutoAddFriendCommand = new RelayCommand<Filter>(x => AddFriendAuto(x));

            AutoSpamFriendCommand = new RelayCommand<Filter>(x => SpamFriendNow(x));
        }

        public ObservableCollection<Filter> AddFriendNearByConfigs
        {
            get { return _addFriendNearByConfigs; }
            set { Set(ref _addFriendNearByConfigs, value); }
        }

        public ObservableCollection<Filter> AutoPostToFriendSessionConfigs
        {
            get { return _autoPostToFriendSessionConfigs; }
            set { Set(ref _autoPostToFriendSessionConfigs, value); }
        }

        public string[] OnlineDevices
        {
            get { return _onlineDevices; }
            set { Set(ref _onlineDevices, value); }
        }

        private ObservableCollection<string> _logs = new ObservableCollection<string>();
        private ObservableCollection<User> _users;
        private User _currentUser;

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                RaisePropertyChanged();
            }
        }

        private string _selectedDevice;

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

        public void Load()
        {
            OnlineDevices = _zaloCommunityService.OnlineDevices;
            Users = new ObservableCollection<User>(new [] { new User { Username = "0979864903", Password = "kimngan12345" } });

            CurrentUser = Users[0];
        }

        private async Task AddFriendAuto(Filter x)
        {
            _settings.User = CurrentUser;
            _settings.DeviceNumber = SelectedDevice;
            var consoleOutput = new ConsoleOutput(_settings.User.Username, _settings.DeviceNumber, "Kết bạn theo vị trí");
            _consoleOutputViewModel.ConsoleOutputs.Add(consoleOutput);

            await _zaloCommunityService.AddFriendNearBy(x, consoleOutput);
        }

        private async Task SpamFriendNow(Filter x)
                    => await _zaloCommunityService.SpamFriend(x);
    }
}