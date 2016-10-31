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

        private AddFriendNearByConfig[] _addFriendNearByConfigs;
        private MessageToFriendConfig[] _autoPostToFriendSessionConfigs;
        private string[] _onlineDevices;

        public ICommand AutoAddFriendCommand { get; }
        public ICommand RefreshAddFriendNearByConfigListCommand { get; }

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

            AutoAddFriendCommand = new RelayCommand<AddFriendNearByConfig>(x => AddFriendAuto(x));
            RefreshAddFriendNearByConfigListCommand = new RelayCommand(() => AddFriendNearByConfigs = _databaseContext.GetAddingFriendConfig());
            RefreshAutoPostToFriendSessionConfigCommand = new RelayCommand(() => AutoPostToFriendSessionConfigs = _databaseContext.GetAutoSpamConfigs());

            AutoSpamFriendCommand = new RelayCommand<MessageToFriendConfig>(x => SpamFriendNow(x));
        }

        public AddFriendNearByConfig[] AddFriendNearByConfigs
        {
            get { return _addFriendNearByConfigs; }
            set { Set(ref _addFriendNearByConfigs, value); }
        }

        public MessageToFriendConfig[] AutoPostToFriendSessionConfigs
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
            AddFriendNearByConfigs = _databaseContext.GetAddingFriendConfig();
            AutoPostToFriendSessionConfigs = _databaseContext.GetAutoSpamConfigs();
        }

        private async Task AddFriendAuto(AddFriendNearByConfig x)
            => await _zaloCommunityService.AddFriendNearBy(x);

        private async Task SpamFriendNow(MessageToFriendConfig x)
                    => await _zaloCommunityService.SpamFriend(x);
    }
}