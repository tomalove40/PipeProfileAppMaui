#if WINDOWS
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Input;
#endif
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PipeProfileAppMaui.Controls
{
    public partial class PipeProfileView
    {
        // для pan‐жеста
        float _lastPanX;
        float _lastPanY;

        void AttachNativeHandlers()
        {
            // подписываем wheel + pointer только на Windows
#if WINDOWS
            if (CanvasView.Handler?.PlatformView is UIElement ui)
            {
                ui.PointerPressed += OnPointerPressed;
                ui.PointerMoved += OnPointerMoved;
                ui.PointerReleased += OnPointerReleased;
                ui.PointerWheelChanged += OnPointerWheelChanged;
            }
#endif
        }


#if WINDOWS

        // состояние пэ́н-и-зум
        bool _isPanning;
        uint _panPointerId;
        SKPoint _lastWorld;
      private void OnEditorKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
                EndEdit();
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var ui = (UIElement)sender;
            var pt = e.GetCurrentPoint(ui);

            // 1) Нажали средней кнопкой — начинаем пан
            if (pt.Properties.IsMiddleButtonPressed)
            {
                _isPanning = true;
                _panPointerId = pt.PointerId;
                ui.CapturePointer(e.Pointer);

                // конвертим DIP→canvas→world
                var dip = new SKPoint((float)pt.Position.X, (float)pt.Position.Y);
                var pix = new SKPoint(dip.X * _editorManager.DipToPixX,
                                       dip.Y * _editorManager.DipToPixY);
                _lastWorld = ViewModel.Transform.InverseMatrix.MapPoint(pix);

                e.Handled = true;
            }
            // 2) Нажали левой — обрабатываем через EditorManager
            else if (pt.Properties.IsLeftButtonPressed)
            {
                var dip = new SKPoint(
                    (float)pt.Position.X,
                    (float)pt.Position.Y);

                _editorManager.HandleTouch(dip, stampRect);
                e.Handled = true;
            }
        }
        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isPanning) return;
            var pt = e.GetCurrentPoint((UIElement)sender);
            if (pt.PointerId != _panPointerId) return;

            // 1) DIP → пиксели канвы
            var pixCur = new SKPoint(
              (float)pt.Position.X * _editorManager.DipToPixX,
              (float)pt.Position.Y * _editorManager.DipToPixY);

            // 2) пиксели → мировые
            var worldCur = ViewModel.Transform.InverseMatrix.MapPoint(pixCur);

            // 3) дельта в мировых
            var worldDx = worldCur.X - _lastWorld.X;
            var worldDy = worldCur.Y - _lastWorld.Y;

            // 4) переводим дельту мира в пиксели канвы
            var screenDx = worldDx * ViewModel.Transform.Zoom;
            var screenDy = worldDy * ViewModel.Transform.Zoom;

            ViewModel.Transform.PanX += screenDx;
            ViewModel.Transform.PanY += screenDy;

            // 5) запоминаем для следующего шага
            _lastWorld = worldCur;

            //ClampTransform();

            CanvasView.InvalidateSurface();
            e.Handled = true;
            PositionEditor();
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint((UIElement)sender);
            if (!_isPanning || pt.PointerId != _panPointerId) return;

            _isPanning = false;
            ((UIElement)sender).ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint((UIElement)sender);
            float f = pt.Properties.MouseWheelDelta > 0 ? 1.1f : 0.9f;

            var pix = new SKPoint(
                (float)pt.Position.X * _editorManager.DipToPixX,
                (float)pt.Position.Y * _editorManager.DipToPixY);

            ViewModel.Transform.ZoomAt(f, pix);
            // в случае, если DPI или размер изменился
            _editorManager.UpdateDipToPix();

            _lastWorld = ViewModel.Transform.InverseMatrix.MapPoint(pix);

            //ClampTransform();

            CanvasView.InvalidateSurface();
            e.Handled = true;
            PositionEditor();
        }
#endif
        void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                // ничего
            }
            else if (e.Status == GestureStatus.Running)
            {
                // точка фокуса в DIP
                var cx = (float)(CanvasView.Width * e.ScaleOrigin.X);
                var cy = (float)(CanvasView.Height * e.ScaleOrigin.Y);
                // в пиксели канвы
                var pivot = new SKPoint(
                    cx * _editorManager.DipToPixX,
                    cy * _editorManager.DipToPixY);

                ViewModel.Transform.ZoomAt((float)e.Scale, pivot);

                //ClampTransform();

                CanvasView.InvalidateSurface();
                _editorManager.PositionEditor();
            }
        }

        void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Started)
            {
                _lastPanX = _lastPanY = 0;
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                // дельта DIP
                float dxDip = (float)e.TotalX - _lastPanX;
                float dyDip = (float)e.TotalY - _lastPanY;

                _lastPanX = (float)e.TotalX;
                _lastPanY = (float)e.TotalY;

                // в пиксели канвы
                float dx = dxDip * _editorManager.DipToPixX;
                float dy = dyDip * _editorManager.DipToPixY;

                ViewModel.Transform.PanX += dx;
                ViewModel.Transform.PanY += dy;

                //ClampTransform();

                CanvasView.InvalidateSurface();
                _editorManager.PositionEditor();
            }
            else if (e.StatusType == GestureStatus.Completed ||
                     e.StatusType == GestureStatus.Canceled)
            {
                _lastPanX = _lastPanY = 0;
            }
        }

        void OnViewSizeChanged(object sender, EventArgs e)
        {
            ViewModel?.UpdatePipeOffsetRatio();
            // новый размер view: Width, Height
            CanvasView.InvalidateSurface();   // перерисовать
        }

        void OnCanvasSizeChanged(object sender, EventArgs e)
        {
            // новый размер холста (DIP)
            this._editorManager.UpdateDipToPix();
            CanvasView.InvalidateSurface();
        }
        //void ClampTransform()
        //{
        //    // Диапазон мировых координат (X и Y) вашей модели
        //    // Например, minX=0, maxX = крайняя точка по дистанции, аналогично для Y
        //    float minX = ViewModel.GroundPoints.Min(p => (float)p.Distance);
        //    float maxX = ViewModel.GroundPoints.Max(p => (float)p.Distance);
        //    // По Y обычно от глубины: minY, maxY
        //    float minY = ViewModel.GroundPoints.Min(p => (float)p.Elevation)
        //               - ViewModel.PipePoints.Max(p => (float)p.Depth);
        //    float maxY = ViewModel.GroundPoints.Max(p => (float)p.Elevation);

        //    // После того как мы рисуем профиль в локальных coords [0…W]/[0…H],
        //    // эти мировые корни масштабируются и смещаются
        //    float z = ViewModel.Transform.Zoom;

        //    // Левый край мира: minX*z + PanX  должен быть ≥ 0
        //    float panMinX = -minX * z;
        //    // Правый край мира: maxX*z + PanX  должен быть ≤ profileArea.Width
        //    float panMaxX = _profileArea.Width - maxX * z;

        //    ViewModel.Transform.PanX = Math.Clamp(ViewModel.Transform.PanX, panMinX, panMaxX);

        //    // Аналогично по Y (если в вашем мире вверх — вниз):
        //    // minY*z + PanY ≥ 0  и  maxY*z + PanY ≤ profileArea.Height
        //    float panMinY = -minY * z;
        //    float panMaxY = _profileArea.Height - maxY * z;

        //    if (panMaxY < panMinY)
        //    {
        //        // просто не Clamp'им, или сбрасываем Transform на дефолт
        //        return;
        //    }

        //    ViewModel.Transform.PanY = Math.Clamp(ViewModel.Transform.PanY, panMinY, panMaxY);
        //}
    }
}
