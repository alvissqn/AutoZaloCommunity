using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Controls
{
    public partial class FilterControl
    {
        public static readonly DependencyProperty SourcesProperty = DependencyProperty.Register(nameof(Sources), typeof(ObservableCollection<Filter>), typeof(FilterControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedFilterProperty = DependencyProperty.Register(nameof(SelectedFilter), typeof(Filter), typeof(FilterControl), new PropertyMetadata(null));
        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(FilterControl), new PropertyMetadata(null));
        public static readonly DependencyProperty NewFilterCommandProperty = DependencyProperty.Register(nameof(NewFilterCommand), typeof(ICommand), typeof(FilterControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ConfigTypeProperty = DependencyProperty.Register(nameof(ConfigType), typeof(string), typeof(FilterControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SaveConfigsProperty = DependencyProperty.Register(nameof(SaveConfigsCommand), typeof(ICommand), typeof(FilterControl), new PropertyMetadata(null));

        private DatabaseContext db = ServiceLocator.Current.GetInstance<DatabaseContext>();

        #region Config Visibility

        [Category("ConfigVisibility")]
        public Visibility SentImageForMaleVisibility
        {
            get { return (Visibility)GetValue(SentImageForMaleVisibilityProperty); }
            set { SetValue(SentImageForMaleVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility SentImageForFemaleVisibility
        {
            get { return (Visibility)GetValue(SentImageForFemaleVisibilityProperty); }
            set { SetValue(SentImageForFemaleVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility IncludePhoneNumbersVisibility
        {
            get { return (Visibility)GetValue(IncludePhoneNumbersVisibilityProperty); }
            set { SetValue(IncludePhoneNumbersVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility ExcludePhoneNumbersVisibility
        {
            get { return (Visibility)GetValue(ExcludePhoneNumbersVisibilityProperty); }
            set { SetValue(ExcludePhoneNumbersVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility AccountNameVisibility
        {
            get { return (Visibility)GetValue(AccountNameVisibilityProperty); }
            set { SetValue(AccountNameVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility ConfigNameVisibility
        {
            get { return (Visibility)GetValue(ConfigNameVisibilityProperty); }
            set { SetValue(ConfigNameVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility ExcludePeopleNamesVisibility
        {
            get { return (Visibility)GetValue(ExcludePeopleNamesVisibilityProperty); }
            set { SetValue(ExcludePeopleNamesVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility FilterAgeRangeVisibility
        {
            get { return (Visibility)GetValue(FilterAgeRangeVisibilityProperty); }
            set { SetValue(FilterAgeRangeVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility GenderSelectionVisibility
        {
            get { return (Visibility)GetValue(GenderSelectionVisibilityProperty); }
            set { SetValue(GenderSelectionVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility IncludedPeopleNamesVisibility
        {
            get { return (Visibility)GetValue(IncludedPeopleNamesVisibilityProperty); }
            set { SetValue(IncludedPeopleNamesVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility LocationsVisibility
        {
            get { return (Visibility)GetValue(LocationsVisibilityProperty); }
            set { SetValue(LocationsVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility NumberOfActionVisibility
        {
            get { return (Visibility)GetValue(NumberOfActionVisibilityProperty); }
            set { SetValue(NumberOfActionVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility TextGreetingForFemaleVisibility
        {
            get { return (Visibility)GetValue(TextGreetingForFemaleVisibilityProperty); }
            set { SetValue(TextGreetingForFemaleVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility TextGreetingForMaleVisibility
        {
            get { return (Visibility)GetValue(TextGreetingForMaleVisibilityProperty); }
            set { SetValue(TextGreetingForMaleVisibilityProperty, value); }
        }

        [Category("ConfigVisibility")]
        public Visibility IgnoreRecentActionBeforeVisibility
        {
            get { return (Visibility)GetValue(IgnoreRecentActionBeforeVisibilityProperty); }
            set { SetValue(IgnoreRecentActionBeforeVisibilityProperty, value); }
        }

        public static readonly DependencyProperty TextGreetingForFemaleVisibilityProperty = DependencyProperty.Register(nameof(TextGreetingForFemaleVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty NumberOfActionVisibilityProperty = DependencyProperty.Register(nameof(NumberOfActionVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IgnoreRecentActionBeforeVisibilityProperty = DependencyProperty.Register(nameof(IgnoreRecentActionBeforeVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty TextGreetingForMaleVisibilityProperty = DependencyProperty.Register(nameof(TextGreetingForMaleVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty LocationsVisibilityProperty = DependencyProperty.Register(nameof(LocationsVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IncludedPeopleNamesVisibilityProperty = DependencyProperty.Register(nameof(IncludedPeopleNamesVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty GenderSelectionVisibilityProperty = DependencyProperty.Register(nameof(GenderSelectionVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty FilterAgeRangeVisibilityProperty = DependencyProperty.Register(nameof(FilterAgeRangeVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty ExcludePeopleNamesVisibilityProperty = DependencyProperty.Register(nameof(ExcludePeopleNamesVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty ConfigNameVisibilityProperty = DependencyProperty.Register(nameof(ConfigNameVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty AccountNameVisibilityProperty = DependencyProperty.Register(nameof(AccountNameVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty ExcludePhoneNumbersVisibilityProperty = DependencyProperty.Register(nameof(ExcludePhoneNumbersVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IncludePhoneNumbersVisibilityProperty = DependencyProperty.Register(nameof(IncludePhoneNumbersVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty SentImageForFemaleVisibilityProperty = DependencyProperty.Register(nameof(SentImageForFemaleVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty SentImageForMaleVisibilityProperty = DependencyProperty.Register(nameof(SentImageForMaleVisibility), typeof(Visibility), typeof(FilterControl), new PropertyMetadata(Visibility.Visible));

        #endregion Config Visibility

        #region Config Header

        [Category("ConfigHeader")]
        public string SentImageForMaleHeader
        {
            get { return (string)GetValue(SentImageForMaleHeaderProperty); }
            set { SetValue(SentImageForMaleHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string SentImageForFemaleHeader
        {
            get { return (string)GetValue(SentImageForFemaleVisibilityProperty); }
            set { SetValue(SentImageForFemaleVisibilityProperty, value); }
        }

        [Category("ConfigHeader")]
        public string IncludePhoneNumbersHeader
        {
            get { return (string)GetValue(IncludePhoneNumbersHeaderProperty); }
            set { SetValue(IncludePhoneNumbersHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string ExcludePhoneNumbersHeader
        {
            get { return (string)GetValue(ExcludePhoneNumbersHeaderProperty); }
            set { SetValue(ExcludePhoneNumbersHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string AccountNameHeader
        {
            get { return (string)GetValue(AccountNameHeaderProperty); }
            set { SetValue(AccountNameHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string ConfigNameHeader
        {
            get { return (string)GetValue(ConfigNameHeaderProperty); }
            set { SetValue(ConfigNameHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string ExcludePeopleNamesHeader
        {
            get { return (string)GetValue(ExcludePeopleNamesHeaderProperty); }
            set { SetValue(ExcludePeopleNamesHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string FilterAgeRangeHeader
        {
            get { return (string)GetValue(FilterAgeRangeHeaderProperty); }
            set { SetValue(FilterAgeRangeHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string GenderSelectionHeader
        {
            get { return (string)GetValue(GenderSelectionHeaderProperty); }
            set { SetValue(GenderSelectionHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string IncludedPeopleNamesHeader
        {
            get { return (string)GetValue(IncludedPeopleNamesHeaderProperty); }
            set { SetValue(IncludedPeopleNamesHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string LocationsHeader
        {
            get { return (string)GetValue(LocationsHeaderProperty); }
            set { SetValue(LocationsHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string NumberOfActionHeader
        {
            get { return (string)GetValue(NumberOfActionHeaderProperty); }
            set { SetValue(NumberOfActionHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string TextGreetingForFemaleHeader
        {
            get { return (string)GetValue(TextGreetingForFemaleHeaderProperty); }
            set { SetValue(TextGreetingForFemaleHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string TextGreetingForMaleHeader
        {
            get { return (string)GetValue(TextGreetingForMaleHeaderProperty); }
            set { SetValue(TextGreetingForMaleHeaderProperty, value); }
        }

        [Category("ConfigHeader")]
        public string IgnoreRecentActionBeforeHeader
        {
            get { return (string)GetValue(IgnoreRecentActionBeforeHeaderProperty); }
            set { SetValue(IgnoreRecentActionBeforeHeaderProperty, value); }
        }

        public static readonly DependencyProperty TextGreetingForFemaleHeaderProperty = DependencyProperty.Register(nameof(TextGreetingForFemaleHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Lời chào bạn nữ"));
        public static readonly DependencyProperty TextGreetingForMaleHeaderProperty = DependencyProperty.Register(nameof(TextGreetingForMaleHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Lời chào bạn nam"));
        public static readonly DependencyProperty SentImageForFemaleHeaderProperty = DependencyProperty.Register(nameof(SentImageForFemaleHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Gửi hình cho bạn nữ"));
        public static readonly DependencyProperty SentImageForMaleHeaderProperty = DependencyProperty.Register(nameof(SentImageForMaleHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Gửi hình cho bạn nam"));
        public static readonly DependencyProperty NumberOfActionHeaderProperty = DependencyProperty.Register(nameof(NumberOfActionHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Số lần kết bạn"));
        public static readonly DependencyProperty IgnoreRecentActionBeforeHeaderProperty = DependencyProperty.Register(nameof(IgnoreRecentActionBeforeHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Chỉ kết bạn sau lần cuối"));
        public static readonly DependencyProperty LocationsHeaderProperty = DependencyProperty.Register(nameof(LocationsHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Vị trí địa lý"));
        public static readonly DependencyProperty IncludedPeopleNamesHeaderProperty = DependencyProperty.Register(nameof(IncludedPeopleNamesHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Bao gồm tài khoản có tên"));
        public static readonly DependencyProperty GenderSelectionHeaderProperty = DependencyProperty.Register(nameof(GenderSelectionHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Giới tính"));
        public static readonly DependencyProperty FilterAgeRangeHeaderProperty = DependencyProperty.Register(nameof(FilterAgeRangeHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Độ tuổi (Ví dụ: 19-45)"));
        public static readonly DependencyProperty ExcludePeopleNamesHeaderProperty = DependencyProperty.Register(nameof(ExcludePeopleNamesHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Trừ tài khoản có tên"));
        public static readonly DependencyProperty ConfigNameHeaderProperty = DependencyProperty.Register(nameof(ConfigNameHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Tên cấu hình"));
        public static readonly DependencyProperty AccountNameHeaderProperty = DependencyProperty.Register(nameof(AccountNameHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Tên tài khoản"));
        public static readonly DependencyProperty ExcludePhoneNumbersHeaderProperty = DependencyProperty.Register(nameof(ExcludePhoneNumbersHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Trừ tài khoản có số đt"));
        public static readonly DependencyProperty IncludePhoneNumbersHeaderProperty = DependencyProperty.Register(nameof(IncludePhoneNumbersHeader), typeof(string), typeof(FilterControl), new PropertyMetadata("Bao gồm tài khoản có số đt"));

        #endregion Config Header

        public string ConfigType
        {
            get { return (string)GetValue(ConfigTypeProperty); }
            set { SetValue(ConfigTypeProperty, value); }
        }

        public ObservableCollection<Filter> Sources
        {
            get { return (ObservableCollection<Filter>)GetValue(SourcesProperty); }
            set { SetValue(SourcesProperty, value); }
        }

        public Filter SelectedFilter
        {
            get { return (Filter)GetValue(SelectedFilterProperty); }
            set { SetValue(SelectedFilterProperty, value); }
        }

        public ICommand NewFilterCommand
        {
            get { return (ICommand)GetValue(NewFilterCommandProperty); }
            set { SetValue(NewFilterCommandProperty, value); }
        }

        public ICommand SaveConfigsCommand
        {
            get { return (ICommand)GetValue(SaveConfigsProperty); }
            set { SetValue(SaveConfigsProperty, value); }
        }

        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }

        public FilterControl()
        {
            InitializeComponent();
            Loaded += FilterControl_Loaded;
        }

        public ICommand StartCommand
        {
            get { return (ICommand)GetValue(StartCommandProperty); }
            set { SetValue(StartCommandProperty, value); }
        }

        public static readonly DependencyProperty StartCommandProperty = DependencyProperty.Register(nameof(StartCommand), typeof(ICommand), typeof(FilterControl), new PropertyMetadata(null));

        private void FilterControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetSource();

            RefreshCommand = new RelayCommand(GetSource);
            NewFilterCommand = new RelayCommand(() =>
            {
                var newFilter = new Filter
                {
                    ConfigName = "#Cau hinh",
                    NumberOfAction = 5
                };

                Sources.Add(newFilter);
                SelectedFilter = newFilter;

            });
            SaveConfigsCommand = new RelayCommand(() =>
            {
                
            });
        }
        private void GetSource()
        {
            Action getSource = () => DispatcherHelper.CheckBeginInvokeOnUI(() => Sources = new ObservableCollection<Filter>(db.GetFilter(ConfigType) ?? new Filter[0]));

            Task.Factory.StartNew(() => getSource());
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            db.SaveFilter(Sources, ConfigType);
            GetSource();
        }
    }
}