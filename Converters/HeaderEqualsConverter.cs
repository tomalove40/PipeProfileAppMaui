using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PipeProfileAppMaui.Converters
{
    public class HeaderEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == (string)parameter;
        }

        //public object ConvertBack(...) => throw new NotImplementedException();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
