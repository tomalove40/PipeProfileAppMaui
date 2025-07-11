using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using PipeProfileAppMaui.ViewModels;

namespace PipeProfileAppMaui.Converters
{
    public class SelectedTabBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value — текущий RibbonTab
            // parameter — SelectedRibbonTab из VM
            if (parameter is RibbonTab selected
                && value is RibbonTab current
                && ReferenceEquals(selected, current))
            {
                return Colors.LightGray; // цвет подсветки
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => throw new NotImplementedException();
    }
}