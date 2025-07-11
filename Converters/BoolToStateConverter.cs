// File: Converters/BoolToStateConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PipeProfileAppMaui.Converters
{
    public class BoolToStateConverter : IValueConverter
    {
        // возвращает имя визуального состояния: "Selected" или "Normal"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return "Selected";
            return "Normal";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => throw new NotImplementedException();
    }
}