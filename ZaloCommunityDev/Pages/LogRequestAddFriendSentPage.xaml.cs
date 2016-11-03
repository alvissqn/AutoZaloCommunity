using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.Data;

namespace ZaloCommunityDev.Pages
{
    public partial class LogRequestAddFriendSentPage
    {
        private readonly DatabaseContext _dbContext = ServiceLocator.Current.GetInstance<DatabaseContext>();

        public LogRequestAddFriendSentPage()
        {
            InitializeComponent();

            Loaded += LogDailyActivityPage_Loaded;
        }

        private void LogDailyActivityPage_Loaded(object sender, System.Windows.RoutedEventArgs e) => LoadData();

        private void LoadData()
        {
            try
            {
                var accountList = _dbContext.GetAccountList().Select(x => x.Username).ToArray();
                AccountFilter.ItemsSource = accountList;
                AccountFilter.SelectedItem = accountList.FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
            DatePicker.SelectedDate = DateTime.Now;
        }

        private void Refresh_Clicked(object sender, System.Windows.RoutedEventArgs e)
            => LogDataGrid.ItemsSource = _dbContext.GetLogRequestAddFriends(DatePicker.SelectedDate ?? DateTime.Now, (string)AccountFilter.SelectedItem);
    }
}