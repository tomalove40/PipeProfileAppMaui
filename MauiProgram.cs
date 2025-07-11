using Microsoft.Extensions.DependencyInjection;
using PipeProfileAppMaui.Services;
using PipeProfileAppMaui.ViewModels;
using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UraniumUI;

namespace PipeProfileAppMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()  // Регистрируем SkiaSharp для MAUI
                .UseMauiCommunityToolkit() // Инициализация CommunityToolkit.Maui
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("gost-type-au.ttf", "GOST_Type_AU");
                });

            //builder.Services.AddSingleton<PipeProfileRenderer>();
            builder.Services.AddSingleton<MainViewModel>();

            builder.Services.AddSingleton<ProfileRenderer>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}