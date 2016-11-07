using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace ZaloCommunityDev.UiAssists
{
    public class GenericConverter : IValueConverter
    {
        private readonly Func<object, object> _convert;
        private readonly Func<object, object> _convertBack;

        public GenericConverter(Func<object, object> convert, Func<object, object> convertBack)
        {
            _convert = convert;
            _convertBack = convertBack;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => _convert(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => _convertBack(value);
    }

    public class GenericWithParamConverter : IValueConverter
    {
        private readonly Func<object, object, object> _convert;
        private readonly Func<object, object, object> _convertBack;

        public GenericWithParamConverter(Func<object, object, object> convert, Func<object, object, object> convertBack)
        {
            _convert = convert;
            _convertBack = convertBack;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => _convert(value, parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => _convertBack(value, parameter);
    }

    public static class UiConverterHelper
    {
        public static IValueConverter TextToUri => Get(text => new Uri((string)text, UriKind.RelativeOrAbsolute));
        public static IValueConverter IsFalse => Get(value => !(bool)value);
        public static IValueConverter InvertBoolean => Get(text => !(bool)text);

        public static IValueConverter TimeSpanNowConverter => Get((timespan) => DateTime.Now.Date.Add((TimeSpan)timespan), (dateTime) => ((DateTime)dateTime) - ((DateTime)dateTime).Date);

        public static IValueConverter IsNotWhiteSpace => Get(text => !string.IsNullOrWhiteSpace(text as string));
        public static IValueConverter IsNotNull => Get(value => value != null);
        public static IValueConverter NullToOpacity => Get(value => value != null ? 1 : 0.5);

        public static IValueConverter HideUsername => Get(value =>
        {
            var len = ((string)value).Length;
            return "******" + ((string)value).Substring(len - 3, 3);
        }, x => x);

        public static IValueConverter HidePassword => Get(value => ((string)value).Substring(0, 3) + "**********", (x) => x);

        public static IValueConverter CountZeroToCollapsed => Get((index) => (int)index == 0 ? Visibility.Collapsed : Visibility.Visible);
        public static IValueConverter ZeroToColumnWidth => Get((value) => (int)value == 0 ? new GridLength(0, GridUnitType.Pixel) : DependencyProperty.UnsetValue);

        public static IValueConverter SimpleGuid => Get(uid => ((Guid)uid).ToString("N").ToLower().Substring(0, 5));

        public static IValueConverter ReturnerdText => Get(isReturned => (bool)isReturned ? "-" : "CHƯA TRẢ");

        public static IValueConverter DisplayTextNullOrEmpty => Get(text => string.IsNullOrWhiteSpace(text as string) ? "-" : text);

        public static IValueConverter CollapsedIfNullOrEmpty => Get(text => string.IsNullOrWhiteSpace(text as string) ? Visibility.Collapsed : Visibility.Visible);
        public static IValueConverter CollapsedIfNull => Get(text => text == null ? Visibility.Collapsed : Visibility.Visible);
        public static IValueConverter CollapsedIfTrue => Get(value => (bool)value ? Visibility.Collapsed : Visibility.Visible);
        public static IValueConverter CollapsedIfFalse => Get(value => !(bool)value ? Visibility.Collapsed : Visibility.Visible);

        public static IValueConverter GetFrameElementTag => Get(x => DependencyProperty.UnsetValue, (frameworkElement) => ((FrameworkElement)frameworkElement).DataContext as string);

        public static IValueConverter EqualsParamsToVisible => Get((x, p) => Convert.ToString(x).Equals(Convert.ToString(p)) ? Visibility.Visible : Visibility.Collapsed);

        private static IValueConverter Create(Func<object, object> convert, Func<object, object> convertBack = null) => new GenericConverter(convert, convertBack);

        private static readonly Dictionary<string, IValueConverter> Converter = new Dictionary<string, IValueConverter>();

        private static IValueConverter Get(Func<object, object> convert, Func<object, object> convertBack = null, [CallerMemberName] string name = null)
        {
            if (!Converter.ContainsKey(name))
            {
                var valueConverter = Create(convert, convertBack);
                Converter.Add(name, valueConverter);
            }

            return Converter[name];
        }

        private static IValueConverter Get(Func<object, object, object> convert, Func<object, object, object> convertBack = null, [CallerMemberName] string name = null)
        {
            if (!Converter.ContainsKey(name))
            {
                var valueConverter = new GenericWithParamConverter(convert, convertBack);
                Converter.Add(name, valueConverter);
            }

            return Converter[name];
        }
    }
}