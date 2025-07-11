using PipeProfileAppMaui.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeProfileAppMaui.Controls
{
    public partial class PipeProfileView
    {
        SKRect _profileArea;

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
         var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColors.White);

            // 0) Применяем текущий Pan/Zoom ко всему рисунку
            canvas.Save();
            canvas.SetMatrix(ViewModel.Transform.Matrix);

            // 1) Letterbox-A4
            float maxW = info.Width - 2 * PageMargin;
            float maxH = info.Height - 2 * PageMargin;
            float a4Ratio = A4WidthMm / A4HeightMm;
            float canvasRatio = maxW / maxH;
            float sheetW = canvasRatio > a4Ratio ? maxH * a4Ratio : maxW;
            float sheetH = sheetW / a4Ratio;
            float left = PageMargin + (maxW - sheetW) / 2;
            float top = PageMargin + (maxH - sheetH) / 2;
            var sheetRect = new SKRect(
                left, top,
                left + sheetW, top + sheetH);

            // Градиент и рамка листа
            using var bg = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(info.Width * 0.2f, info.Height * 0.2f),
                    new[] { SKColors.LightSkyBlue, SKColors.White },
                    null, SKShaderTileMode.Clamp)
            };
            canvas.Save();
            canvas.ClipRect(sheetRect, SKClipOperation.Difference);
            canvas.DrawRect(-info.Width, -info.Height, info.Width * 3, info.Height * 3, bg);
            canvas.Restore();
            using (var frame = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsAntialias = true
            })
                canvas.DrawRect(sheetRect, frame);

            // 1) Letterbox-A4
            _sheetRect = DrawingHelpers.CalculateLetterbox(
                e.Info.Size,
                PageConfig.PageMargin,
                PageConfig.A4WidthMm / PageConfig.A4HeightMm);

            // 2) ГОСТ-рамка
            float pxPerMm = _sheetRect.Width / PageConfig.A4WidthMm;
            _gostRect = new SKRect(
                _sheetRect.Left + 20f * pxPerMm,
                _sheetRect.Top + 5f * pxPerMm,
                _sheetRect.Right - 5f * pxPerMm,
                _sheetRect.Bottom - 5f * pxPerMm);

            // 2) Профильная область
            //float pxPerMm = sheetRect.Width / A4WidthMm;
            float stampH = 55f * pxPerMm;
            // 3) Профильная область внутри gostRect
            _profileArea = new SKRect(
                _gostRect.Left + PageConfig.ProfileInset,
                _gostRect.Top + PageConfig.ProfileInset,
                _gostRect.Right - PageConfig.ProfileInset,
                _gostRect.Bottom - PageConfig.ProfileInset);

            // ... после расчёта profileArea ...
            //_profileArea = profileArea;

            // 3) Рисуем профиль
            _renderer.Render(
                canvas,
                _profileArea,
                ViewModel.GroundPoints,
                ViewModel.PipePoints,
                ViewModel.InterpolateGroundElevation);

            var gostRect = new SKRect(
            sheetRect.Left + 20f * pxPerMm,
            sheetRect.Top + 5f * pxPerMm,
            sheetRect.Right - 5f * pxPerMm,
            sheetRect.Bottom - 5f * pxPerMm);

            // 3) Рисуем контур ГОСТ-рамки
            using var framePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsAntialias = true
            };
            canvas.DrawRect(gostRect, framePaint);

            // 4) Рисуем штамп
            var gost = new SKRect(
                sheetRect.Left + 20f * pxPerMm,
                sheetRect.Top + 5f * pxPerMm,
                sheetRect.Right - 5f * pxPerMm,
                sheetRect.Bottom - 5f * pxPerMm);
            float stampW = 180f * pxPerMm;
            stampRect = new SKRect(
                gost.Right - stampW,
                gost.Bottom - stampH,
                gost.Right,
                gost.Bottom);
            _stamp.Draw(canvas, stampRect);

            canvas.Restore();        }
    }
}
