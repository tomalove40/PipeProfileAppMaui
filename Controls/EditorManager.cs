using System;
using System.Linq;
using Microsoft.Maui.Controls;
using PipeProfileAppMaui.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using Microsoft.Maui.Layouts;
using SkiaSharp.Views.Maui.Controls;
using PipeProfileAppMaui.ViewModels;

namespace PipeProfileAppMaui.Controls
{
    /// <summary>
    /// Управляет созданием, позиционированием и закрытием редактора штампа.
    /// </summary>
    public class EditorManager
    {
        readonly AbsoluteLayout _root;
        readonly SKCanvasView _canvas;
        readonly Stamp _stamp;
        MainViewModel _vm;
        readonly Action _invalidate;

        Editor _editor;
        int _row, _col;
        SKRect _cellWorld;
        float _dipToPixX = 1, _dipToPixY = 1;

        public float DipToPixX => _dipToPixX;
        public float DipToPixY => _dipToPixY;

        public EditorManager(
            AbsoluteLayout root,
            SKCanvasView canvas,
            Stamp stamp,
            MainViewModel vm,
            Action invalidateSurface)
        {
            _root = root;
            _canvas = canvas;
            _stamp = stamp;
            _vm = vm;
            _invalidate = invalidateSurface;

            // Пересчитать DPI → пиксели при изменении размера
            _canvas.SizeChanged += (_, _) => UpdateDipToPix();
        }

        /// <summary>
        /// Перевычисляет коэффициенты DIP→Pixel.
        /// </summary>
        /// 

        public void SetViewModel(MainViewModel vm)
        {
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        }

        public void UpdateDipToPix()
        {
            var sz = _canvas.CanvasSize;
            if (_canvas.Width > 0 && _canvas.Height > 0)
            {
                _dipToPixX = sz.Width / (float)_canvas.Width;
                _dipToPixY = sz.Height / (float)_canvas.Height;
            }
        }

        /// <summary>
        /// Обрабатывает касание на CanvasView.
        /// Вызывает ShowEditor или EndEdit по необходимости.
        /// </summary>
        public void HandleTouch(SKPoint locationInDIP, SKRect stampRect)
        {
            // 1) Переводим DIP → пиксели канвы
            var pix = new SKPoint(locationInDIP.X * _dipToPixX,
                                  locationInDIP.Y * _dipToPixY);

            // 2) Преобразуем в мировые координаты
            var world = _vm.Transform.InverseMatrix.MapPoint(pix);

            // 3) Если редактор открыт и тап вне текущей ячейки — закрыть
            if (_editor != null && !_cellWorld.Contains(world))
            {
                EndEdit();
                return;
            }

            // 4) Проверяем попадание в штамп
            var hit = _stamp.HitTest(world, stampRect);
            if (hit.HasValue)
            {
                ShowEditor(hit.Value.row, hit.Value.col, stampRect);
            }
        }
        /// <summary>
        /// Открывает Editor над ячейкой (row,col).
        /// </summary>
        void ShowEditor(int row, int col, SKRect stampRect)
        {
            // Закрываем предыдущий
            if (_editor != null)
                EndEdit();

            _row = row;
            _col = col;
            _cellWorld = _stamp.GetCellRect(row, col, stampRect);

            // Создаем Editor и биндим текст напрямую к VM
            var cellVm = _vm.StampCells[row][col];
            var editor = new Editor
            {
                BackgroundColor = Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };
            editor.SetBinding(Editor.TextProperty,
                new Binding(nameof(StampCell.Text),
                            source: cellVm,
                            mode: BindingMode.TwoWay));



            // Закрытие по потере фокуса
            editor.Unfocused += (_, __) => EndEdit();

            _editor = editor;

            editor.AutoSize = EditorAutoSizeOption.TextChanges;

            PositionEditor();

            AbsoluteLayout.SetLayoutFlags(editor, AbsoluteLayoutFlags.None);
            _root.Children.Add(editor);
            editor.Focus();
        }

        /// <summary>
        /// Переставляет Editor по текущему Pan/Zoom.
        /// </summary>
        public void PositionEditor()
        {
            if (_editor == null)
                return;

            // world → canvas → DIP
            var p0 = _vm.Transform.Matrix.MapPoint(_cellWorld.Left, _cellWorld.Top);
            var p1 = _vm.Transform.Matrix.MapPoint(_cellWorld.Right, _cellWorld.Bottom);

            double x = p0.X / _dipToPixX;
            double y = p0.Y / _dipToPixY;
            double w = (p1.X - p0.X) / _dipToPixX;
            double h = (p1.Y - p0.Y) / _dipToPixY;

            AbsoluteLayout.SetLayoutBounds(_editor, new Rect(x, y, w, h));
        }

        /// <summary>
        /// Закрывает Editor, не трогая модель — двусторонний биндинг уже записал текст.
        /// </summary>
        public void EndEdit()
        {
            if (_editor == null)
                return;

            _root.Children.Remove(_editor);
            _editor = null;

            // перерисовать штамп
            _invalidate();
        }
    }
}