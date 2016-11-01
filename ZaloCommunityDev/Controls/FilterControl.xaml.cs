using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Controls
{
    public partial class FilterControl
    {
        public static readonly DependencyProperty SourcesProperty = DependencyProperty.Register(nameof(Sources), typeof(IEnumerable<Filter>), typeof(FilterControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedFilterProperty = DependencyProperty.Register(nameof(SelectedFilter), typeof(Filter), typeof(FilterControl), new PropertyMetadata(null));
        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(FilterControl), new PropertyMetadata(null));


        public IEnumerable<Filter> Sources
        {
            get { return (IEnumerable<Filter>)GetValue(SourcesProperty); }
            set { SetValue(SourcesProperty, value); }
        }

        public Filter SelectedFilter
        {
            get { return (Filter)GetValue(SelectedFilterProperty); }
            set { SetValue(SelectedFilterProperty, value); }
        }


        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }

        public FilterControl()
        {
            InitializeComponent();
        }
    }
}