using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using PipeProfileAppMaui.ViewModels;
using PipeProfileAppMaui.Services;
using System.Diagnostics;

namespace PipeProfileAppMaui;

public partial class MainPage : ContentPage
{
    public MainPage()           // ← **этот** конструктор Shell будет вызывать
    {
        InitializeComponent();

        // получаем VM из Locator
        var vm = new ViewModelLocator().MainViewModel;
        BindingContext = vm;
    }

    void TabsCollectionView_Loaded(object sender, EventArgs e)
    {
        // Получаем локатор из ресурсов страницы
        var locator = (ViewModelLocator)Resources["Locator"];
        var ribbonVm = locator.RibbonViewModel;

        // Устанавливаем в VM выбранную табу
        if (ribbonVm.RibbonTabs.Count > 0)
        {
            var first = ribbonVm.RibbonTabs[0];
            ribbonVm.SelectedRibbonTab = first;

            // И обновляем сам CollectionView, чтобы он подсветил её
            TabsCollectionView.SelectedItem = first;
        }
    }
    // Удалите или закомментируйте конструктор MainPage(MainViewModel)
}