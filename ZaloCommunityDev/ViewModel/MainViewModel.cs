using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System;
using ZaloCommunityDev.Services;
using ZaloCommunityDev.Models;
using ZaloCommunityDev.DAL;
using System.Threading.Tasks;

namespace ZaloCommunityDev.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        string[] _onlineDevices;

        public string[] OnlineDevices { get { return _onlineDevices; } set { Set(ref _onlineDevices, value); } }

        private ZaloCommunityService _service;

        public ICommand StartAvdCommand { get; }
        public ICommand RefreshAvdListCommand { get; }
        public ICommand RefreshAutoAddFriendConfigListCommand { get; }
        public ICommand RefreshAutoPostToFriendSessionConfigCommand { get; }
        public ICommand AutoAddFriendCommand { get; }
        public ICommand AutoSpamFriendCommand { get; }


        AddingFriendConfig[] _addingFriendConfigs;
        public AddingFriendConfig[] AddingFriendConfigs { get { return _addingFriendConfigs; } set { Set(ref _addingFriendConfigs, value); } }

        AutoPostToFriendSessionConfig[] _AutoPostToFriendSessionConfigs;
        public AutoPostToFriendSessionConfig[] AutoPostToFriendSessionConfigs { get { return _AutoPostToFriendSessionConfigs; } set { Set(ref _AutoPostToFriendSessionConfigs, value); } }

        public void Load()
        {
            OnlineDevices = _service.OnlineDevices;
            AddingFriendConfigs = _dbContext.GetAddingFriendConfig();
            AutoPostToFriendSessionConfigs = _dbContext.GetAutoSpamConfigs();
        }
        DatabaseContext _dbContext;
        public MainViewModel(ZaloCommunityService service, DatabaseContext dbContext)
        {
            _service = service;
            _dbContext = dbContext;

            Load();

            StartAvdCommand = new RelayCommand<string>(_service.StartAvd);
            RefreshAvdListCommand = new RelayCommand(() => OnlineDevices = _service.OnlineDevices);

            AutoAddFriendCommand = new RelayCommand<AddingFriendConfig>(x => AddFriendAuto(x));
            RefreshAutoAddFriendConfigListCommand = new RelayCommand(() => AddingFriendConfigs = _dbContext.GetAddingFriendConfig());
            RefreshAutoPostToFriendSessionConfigCommand = new RelayCommand(() => AutoPostToFriendSessionConfigs = _dbContext.GetAutoSpamConfigs());

            AutoSpamFriendCommand = new RelayCommand<AutoPostToFriendSessionConfig>(x => SpamFriendNow(x));
        }

        private async Task SpamFriendNow(AutoPostToFriendSessionConfig x)
        {
            await _service.SpamFriend(x);
        }

        private async Task AddFriendAuto(AddingFriendConfig x)
        {
            await _service.AddFriendNearBy(x);
        }
    }
}