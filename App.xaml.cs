using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using PipeProfileAppMaui.ViewModels;
using System.Diagnostics;

namespace PipeProfileAppMaui
{
    public partial class App : Application
    {
        // 1) Статический локатор, чтобы к нему можно было обратиться из XAML
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}