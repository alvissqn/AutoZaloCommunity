using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.Data;

namespace ZaloCommunityDev.Pages
{
    public partial class LogDailyActivityPage
    {
        private readonly DatabaseContext _dbContext = ServiceLocator.Current.GetInstance<DatabaseContext>();

        public LogDailyActivityPage()
        {
            InitializeComponent();
            Loaded += LogDailyActivityPage_Loaded;
        }

        private void LoadData()
        {
            DataGridDailyLog.ItemsSource = _dbContext.GetDailyActivity();

            try
            {
                var accountList = _dbContext.GetAccountList().Select(x => x.Username).ToArray();
                AccountFilter.ItemsSource = accountList;
            }
            catch (Exception ex)
            {
            }
            DatePicker.SelectedDate = null;
        }

        private void LogDailyActivityPage_Loaded(object sender, System.Windows.RoutedEventArgs e) => LoadData();

        private void Refresh_Click(object sender, System.Windows.RoutedEventArgs e)
            => DataGridDailyLog.ItemsSource = _dbContext.GetDailyActivity(DatePicker.SelectedDate, (string)AccountFilter.SelectedItem);
    }
}