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

        private void LogDailyActivityPage_Loaded(object sender, System.Windows.RoutedEventArgs e) => LoadData();

        private void LoadData() => DataGridDailyLog.ItemsSource = _dbContext.GetDailyActivity();

        private void Refresh_Click(object sender, System.Windows.RoutedEventArgs e) => LoadData();
    }
}