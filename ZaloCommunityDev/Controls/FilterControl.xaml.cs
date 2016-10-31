using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ZaloCommunityDev.Controls
{
    public partial class FilterControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<>), typeof(FilterControl), (PropertyMetadata)new FrameworkPropertyMetadata((object)null, new PropertyChangedCallback(OnItemsSourceChanged)));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = (FilterControl)d;
        }



        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }

        public static readonly DependencyProperty RefreshCommandProperty =
            DependencyProperty.Register("RefreshCommand", typeof(ICommand), typeof(FilterControl), new PropertyMetadata(null));



        public FilterControl()
        {
            InitializeComponent();
        }
    }
}