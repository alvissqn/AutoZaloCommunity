using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using ZaloCommunityDev.Services;
using ZaloCommunityDev.Models;
using ZaloCommunityDev.DAL;
using System.Threading.Tasks;

namespace ZaloCommunityDev.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly ZaloCommunityService _zaloCommunityService;

        private AddingFriendConfig[] _addingFriendConfigs;
        private AutoPostToFriendSessionConfig[] _autoPostToFriendSessionConfigs;
        private string[] _onlineDevices;

        public ICommand AutoAddFriendCommand { get; }
        public ICommand RefreshAutoAddFriendConfigListCommand { get; }

        public ICommand AutoSpamFriendCommand { get; }
        public ICommand RefreshAutoPostToFriendSessionConfigCommand { get; }

        public ICommand RefreshAvdListCommand { get; }
        public ICommand StartAvdCommand { get; }


        public MainViewModel(ZaloCommunityService service, DatabaseContext dbContext)
        {
            _zaloCommunityService = service;
            _databaseContext = dbContext;

            Load();

            StartAvdCommand = new RelayCommand<string>(_zaloCommunityService.StartAvd);
            RefreshAvdListCommand = new RelayCommand(() => OnlineDevices = _zaloCommunityService.OnlineDevices);

            AutoAddFriendCommand = new RelayCommand<AddingFriendConfig>(x => AddFriendAuto(x));
            RefreshAutoAddFriendConfigListCommand = new RelayCommand(() => AddingFriendConfigs = _databaseContext.GetAddingFriendConfig());
            RefreshAutoPostToFriendSessionConfigCommand = new RelayCommand(() => AutoPostToFriendSessionConfigs = _databaseContext.GetAutoSpamConfigs());

            AutoSpamFriendCommand = new RelayCommand<AutoPostToFriendSessionConfig>(x => SpamFriendNow(x));
        }

        public AddingFriendConfig[] AddingFriendConfigs
        {
            get { return _addingFriendConfigs; }
            set { Set(ref _addingFriendConfigs, value); }
        }

        public AutoPostToFriendSessionConfig[] AutoPostToFriendSessionConfigs
        {
            get { return _autoPostToFriendSessionConfigs; }
            set { Set(ref _autoPostToFriendSessionConfigs, value); }
        }

        public string[] OnlineDevices
        {
            get { return _onlineDevices; }
            set { Set(ref _onlineDevices, value); }
        }

        public void Load()
        {
            OnlineDevices = _zaloCommunityService.OnlineDevices;
            AddingFriendConfigs = _databaseContext.GetAddingFriendConfig();
            AutoPostToFriendSessionConfigs = _databaseContext.GetAutoSpamConfigs();
        }

        private async Task AddFriendAuto(AddingFriendConfig x)
            => await _zaloCommunityService.AddFriendNearBy(x);

        private async Task SpamFriendNow(AutoPostToFriendSessionConfig x)
                    => await _zaloCommunityService.SpamFriend(x);
    }
}