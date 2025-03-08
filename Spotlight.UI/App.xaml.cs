using Microsoft.Extensions.DependencyInjection;
using Spotlight.Abstractions;
using Spotlight.Services;
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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IEventService, EventService>();
            services.AddSingleton<IErrorService, ErrorService>
        }
    }

    public class ComboBoxWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var width = ((double)value) - 10;
            return width < 0 ? 100 : width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) + 10;
        }
    }
}