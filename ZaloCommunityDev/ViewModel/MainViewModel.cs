﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.Models;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ConsoleOutputViewModel _consoleOutputViewModel;
        private readonly Settings _settings;
        private readonly ZaloCommunityService _zaloCommunityService;
        private readonly DatabaseContext _dbContext;
        private string[] _onlineDevices;
        private string _selectedDevice;
        private ObservableCollection<User> _users;

        public MainViewModel(ZaloCommunityService service, DatabaseContext dbContext, Settings settings, ConsoleOutputViewModel consoleOutputViewModel)
        {
            _zaloCommunityService = service;
            _dbContext = dbContext;
            _settings = settings;
            _consoleOutputViewModel = consoleOutputViewModel;

            Load();

            RefreshAvdListCommand = new RelayCommand(() => OnlineDevices = _zaloCommunityService.OnlineDevices);

            AutoAddFriendByPhoneCommand = new RelayCommand<Filter>(AutoAddFriendByPhoneInvoke);
            AutoAddFriendNearByCommand = new RelayCommand<Filter>(AutoAddFriendNearByInvoke);
            AutoSendMessageToFriendCommand = new RelayCommand<Filter>(AutoSendMessageToFriendInvoke);
            AutoSendMessageToStrangerByPhoneCommand = new RelayCommand<Filter>(AutoSendMessageToStrangerByPhoneInvoke);
            AutoSendMessageToStrangerNearByCommand = new RelayCommand<Filter>(AutoSendMessageToStrangerNearByInvoke);

            SearchAllContactCommand = new RelayCommand<string>(SearchAllContact);
        }

        #region Commands

        public ICommand AutoAddFriendByPhoneCommand { get; }
        public ICommand AutoAddFriendNearByCommand { get; }
        public ICommand AutoSendMessageToFriendCommand { get; }
        public ICommand AutoSendMessageToStrangerByPhoneCommand { get; }
        public ICommand AutoSendMessageToStrangerNearByCommand { get; }
        public ICommand RefreshAvdListCommand { get; }

        public ICommand SearchAllContactCommand { get; }

        #endregion Commands

        #region Properties

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

        #region Methods

        public void Load()
        {
            OnlineDevices = _zaloCommunityService.OnlineDevices;
            Users = new ObservableCollection<User>(_dbContext.GetAccountList());
        }

        private ConsoleOutput CreateConsoleOutput(string type)
        {
            _settings.AndroidDebugBridgeOsWorkingLocation = _zaloCommunityService.AndroidDebugBridgeOsLocation;
            _settings.MaxFriendAddedPerDay = int.Parse(ConfigurationManager.AppSettings["MaxFriendAddedPerDay"]);
            _settings.MaxMessageStrangerPerDay = int.Parse(ConfigurationManager.AppSettings["MaxMessageStrangerPerDay"]);
            _settings.User = Users.FirstOrDefault(x => x.IsActive);
            if (_settings.User == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản");

                return null;
            }
            _settings.DeviceNumber = SelectedDevice;
            if (string.IsNullOrWhiteSpace(_settings.DeviceNumber))
            {
                MessageBox.Show("Vui lòng chọn một thiết bị android đang chạy. Kiểm tra thiết bị đã chạy chưa");

                return null;
            }

            var consoleOutput = new ConsoleOutput(_settings.User.Username, _settings.DeviceNumber, type);
            _consoleOutputViewModel.ConsoleOutputs.Add(consoleOutput);

            return consoleOutput;
        }

        #endregion Methods

        #region Auto

        private async void SearchAllContact(string obj)
        {
            var consoleOutput = CreateConsoleOutput("Quét danh bạ");
            if (consoleOutput == null)
                return;

            await _zaloCommunityService.SearchAllContact(consoleOutput);
        }

        private async void AutoAddFriendByPhoneInvoke(Filter filter)
        {
            string reasonFail;
            if (!filter.IsValid(out reasonFail))
            {
                MessageBox.Show("Kiểm tra lại bộ lọc. Lỗi: " + reasonFail);
                return;
            }

            var consoleOutput = CreateConsoleOutput("Kết bạn theo số đt");
            if (consoleOutput == null)
                return;

            await _zaloCommunityService.AddFriendByPhone(filter, consoleOutput);
        }

        private async void AutoAddFriendNearByInvoke(Filter filter)
        {
            string reasonFail;
            if (!filter.IsValid(out reasonFail))
            {
                MessageBox.Show("Kiểm tra lại bộ lọc. Lỗi: " + reasonFail);
                return;
            }

            var consoleOutput = CreateConsoleOutput("Kết bạn theo vị trí");
            if (consoleOutput == null)
                return;

            await _zaloCommunityService.AddFriendNearBy(filter, consoleOutput);
        }

        private async void AutoSendMessageToFriendInvoke(Filter filter)
        {
            string reasonFail;
            if (!filter.IsValid(out reasonFail))
            {
                MessageBox.Show("Kiểm tra lại bộ lọc. Lỗi: " + reasonFail);
                return;
            }

            var consoleOutput = CreateConsoleOutput("Gửi tin nhắn cho bạn");
            if (consoleOutput == null)
                return;

            await _zaloCommunityService.SendMessageToFriend(filter, consoleOutput);
        }

        private async void AutoSendMessageToStrangerByPhoneInvoke(Filter filter)
        {
            string reasonFail;
            if (!filter.IsValid(out reasonFail))
            {
                MessageBox.Show("Kiểm tra lại bộ lọc. Lỗi: " + reasonFail);
                return;
            }

            filter.NumberOfAction = filter.IncludePhoneNumbers.ZaloSplitText().Length;

            var consoleOutput = CreateConsoleOutput("Gửi tin nhắn theo số đt");
            if (consoleOutput == null)
                return;

            await _zaloCommunityService.SendMessageToStrangerByPhone(filter, consoleOutput);
        }

        private async void AutoSendMessageToStrangerNearByInvoke(Filter filter)
        {
            string reasonFail;
            if (!filter.IsValid(out reasonFail))
            {
                MessageBox.Show("Kiểm tra lại bộ lọc. Lỗi: " + reasonFail);
                return;
            }

            var consoleOutput = CreateConsoleOutput("Gửi tin nhắn theo vị trí");
            if (consoleOutput == null)
                return;

            await _zaloCommunityService.SendMessageToStrangerNearBy(filter, consoleOutput);
        }

        #endregion Auto
    }
}