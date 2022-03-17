using System;
using System.Globalization;
using System.Windows.Data;

namespace Achievement.Exporter.Plugin.View.Converter
{
    internal class BooleanToStringSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool boolean)
            {
                flag = boolean;
            }
            else if (value is bool?)
            {
                bool? flag2 = (bool?)value;
                flag = flag2.HasValue && flag2.Value;
            }
            if (parameter is string @string)
            {
                string[] ps = @string.Split(';');

                return !flag ? ps[0] : ps[1];
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
