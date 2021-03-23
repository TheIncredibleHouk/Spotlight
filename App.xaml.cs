using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spotlight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    }

    public class ComboBoxWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) -10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) + 10;        }
    }
}