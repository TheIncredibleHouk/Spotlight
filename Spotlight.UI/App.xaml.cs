using Microsoft.Extensions.DependencyInjection;
using Spotlight.Abstractions;
using Spotlight.Abstractions.Renderers;
using Spotlight.Renderers;
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
        public static IServiceProvider Services { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IEventService, EventService>();
            services.AddSingleton<IErrorService, ErrorService>();
            services.AddScoped<IGraphicsManager, GraphicsManager>();
            services.AddScoped<ILevelDataManager, LevelDataManager>();
            services.AddScoped<IWorldDataManager, WorldDataManager>();
            services.AddSingleton<IClipboardService, ClipboardService>();
            services.AddSingleton<ICompressionService, CompressionService>();
            services.AddSingleton<IGameObjectService, GameObjectService>();
            services.AddSingleton<IGraphicsService, GraphicsService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddSingleton<ILevelService, LevelService>();
            services.AddSingleton<IPaletteService, PaletteService>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IRomService, RomService>();
            services.AddSingleton<ITextService, TextService>();
            services.AddSingleton<ITileService, TileService>();
            services.AddSingleton<IWorldService, WorldService>();
            services.AddScoped<ILevelRenderer, LevelRenderer>();
            services.AddScoped<IWorldRenderer, WorldRenderer>();
            services.AddScoped<IPaletteRenderer, PaletteRenderer>();
            Services = services.BuildServiceProvider();
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