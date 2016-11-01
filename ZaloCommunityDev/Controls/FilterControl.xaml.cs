using System;
using System.Collections.ObjectModel;
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
        public static readonly DependencyProperty ConfigTypeProperty = DependencyProperty.Register(nameof(ConfigType), typeof(string), typeof(FilterControl), new PropertyMetadata(null, new PropertyChangedCallback(OnTypeChanged)));
        public static readonly DependencyProperty SaveConfigsProperty = DependencyProperty.Register(nameof(SaveConfigsCommand), typeof(ICommand), typeof(FilterControl), new PropertyMetadata(null));
        private DatabaseContext db = ServiceLocator.Current.GetInstance<DatabaseContext>();

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var filter = (FilterControl)d;
            var key = e.NewValue as string;
            switch (key)
            {
                case "AutoAddFriendNearByPage":
                case "AutoAddFriendByPhonePage":
                    filter.SentImageForFemaleGrid.Visibility = Visibility.Collapsed;
                    filter.SentImageForMaleGrid.Visibility = Visibility.Collapsed;
                    filter.IncludedPeopleNamesGrid.Visibility = Visibility.Collapsed;
                    filter.ExcludePeopleNamesGrid.Visibility = Visibility.Collapsed;
                    filter.IncludePhoneNumbersGrid.Visibility = Visibility.Collapsed;
                    filter.ExcludePhoneNumbersGrid.Visibility = Visibility.Collapsed;
                    filter.NumberOfActionHeader.Text = "Số bạn";

                    break;

                case "AutoSendMessageToFriendPage":

                    break;

                case "AutoSendMessageToStrangerByPhonePage":
                    filter.ExcludePhoneNumbersGrid.Visibility = Visibility.Collapsed;

                    filter.IncludedPeopleNamesGrid.Visibility = Visibility.Collapsed;
                    filter.ExcludePeopleNamesGrid.Visibility = Visibility.Collapsed;

                    break;

                case "AutoSendMessageToStrangerNearByPage":
                    filter.IncludePhoneNumbersGrid.Visibility = Visibility.Collapsed;
                    filter.ExcludePhoneNumbersGrid.Visibility = Visibility.Collapsed;

                    filter.IncludedPeopleNamesGrid.Visibility = Visibility.Collapsed;
                    filter.ExcludePeopleNamesGrid.Visibility = Visibility.Collapsed;
                    break;

                default:
                    return;
            }
        }

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

        private void FilterControl_Loaded(object sender, RoutedEventArgs e)
        {
            Action getSource = () => DispatcherHelper.CheckBeginInvokeOnUI(() => Sources = new ObservableCollection<Filter>(db.GetFilter(ConfigType) ?? new Filter[0]));

            Task.Factory.StartNew(() => getSource());

            RefreshCommand = new RelayCommand(getSource);
            NewFilterCommand = new RelayCommand(() =>
            {
                var newFilter = new Filter { ConfigName = "#Cau hinh" };
                Sources.Add(newFilter);
                SelectedFilter = newFilter;
            });
            SaveConfigsCommand = new RelayCommand(() =>
            {
                db.SaveFilter(Sources, ConfigType);
                getSource();
            });
        }
    }
}