using System;
using System.Globalization;
using System.Windows.Data;

namespace Achievement.Exporter.Plugin.View.Converter
{
    internal class NavigateUriAddtionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string @string)
            {
                if (!@string.StartsWith("https"))
                {
                    @string = $"https://{@string}";
                }

                return new Uri(@string);
            }
            return new Uri(string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
