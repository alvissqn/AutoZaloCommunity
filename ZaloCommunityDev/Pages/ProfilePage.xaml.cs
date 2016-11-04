using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.Data;

namespace ZaloCommunityDev.Pages
{
    public partial class ProfilePage
    {
        private readonly DatabaseContext _dbContext = ServiceLocator.Current.GetInstance<DatabaseContext>();

        public ProfilePage()
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
        }

        private void Refresh_Clicked(object sender, System.Windows.RoutedEventArgs e)
            => ContactDataGrid.ItemsSource = _dbContext.GetAllProfile((string)AccountFilter.SelectedItem);
    }
}