using ModernWpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Achievement.Exporter.Plugin
{
    internal class BooleanToBrushSelectorConverter : IValueConverter
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
                string p = !flag ? ps[0] : ps[1];

                if (p == "null")
                {
                    p = ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark ? "White" : "Black";
                }
                else if (!p.StartsWith("#"))
                {
                    p = $"#{p}";
                }
                return new BrushConverter().ConvertFromString(p)!;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
