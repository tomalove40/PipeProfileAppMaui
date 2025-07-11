using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using PipeProfileAppMaui.Helpers;
using PipeProfileAppMaui.Models;
using PipeProfileAppMaui.Services;
using PipeProfileAppMaui.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PipeProfileAppMaui.Controls
{
    public partial class PipeProfileView : ContentView
    {
        SKRect _sheetRect, _gostRect;
        // 1) VM-свойство и биндинг
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(
                nameof(ViewModel),
                typeof(MainViewModel),
                typeof(PipeProfileView),
                propertyChanged: OnViewModelChanged);

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // 2) Сервисы и компоненты
        readonly ProfileRenderer _renderer = new ProfileRenderer();
        readonly EditorManager _editorManager;
        readonly Stamp _stamp;
        SKRect stampRect;

        // 3) Константы для страницы
        const float PageMargin = 10f;
        const float A4WidthMm = 297f;
        const float A4HeightMm = 210f;


        static void OnViewModelChanged(BindableObject bindable, object _, object newValue)
        {
            var ctl = (PipeProfileView)bindable;

            if (newValue is not MainViewModel vm)
                return;

            ctl.BindingContext = vm;

            // защищаемся от того, что _stamp ещё не создан
            if (ctl._stamp != null)
                ctl._stamp.StampCells = vm.StampCells;

            if (ctl._editorManager != null)
                ctl._editorManager.SetViewModel(vm);

            ctl.CanvasView?.InvalidateSurface();
        }

        public double CalculateSheetToPipeRatio()
        {
            if (ViewModel.PipePoints == null || ViewModel.PipePoints.Count == 0)
                return 0.0; // Нет точек трубы

            // 1) Длина листа (в мм)
            float sheetWidthMm = PageConfig.A4WidthMm; // 297 мм

            // 2) Получаем первую точку трубы (PipePoint.Distance в метрах или мм)
            float pipeStartDistance = (float)ViewModel.PipePoints[0].Distance;

            // 3) Минимальная координата X в мировых координатах (для профиля)
            float minX = ViewModel.GroundPoints.Min(p => (float)p.Distance);

            // 4) Масштаб профиля (пиксели на единицу мира)
            float spanX = ViewModel.GroundPoints.Max(p => (float)p.Distance) - minX;
            float scaleX = _profileArea.Width / spanX; // Пиксели на единицу мира

            // 5) Переводим расстояние от начала профиля до трубы в пиксели
            float pipeStartPx = (pipeStartDistance - minX) * scaleX;

            // 6) Расстояние от левого края листа до начала профиля (в пикселях)
            float profileOffsetPx = _profileArea.Left - _sheetRect.Left;

            // 7) Полное расстояние от левого края листа до начала трубы (в пикселях)
            float totalDistancePx = profileOffsetPx + pipeStartPx;

            // 8) Переводим расстояние в миллиметры
            float pxPerMm = _sheetRect.Width / PageConfig.A4WidthMm;
            float totalDistanceMm = totalDistancePx / pxPerMm;

            // 9) Вычисляем отношение
            if (totalDistanceMm == 0) return 0.0; // Избегаем деления на ноль
            return Math.Round(sheetWidthMm / totalDistanceMm, 2); // Округляем до 2 знаков
        }


    }
}