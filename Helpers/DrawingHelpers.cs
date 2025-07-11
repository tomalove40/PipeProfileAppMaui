using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeProfileAppMaui.Helpers
{
    public static class DrawingHelpers
    {
        // 1) Рассчитать letterbox-область нужного отношения
        public static SKRect CalculateLetterbox(
            SKSize canvasSize, float margin, float targetRatio)
        {
            float maxW = canvasSize.Width - 2 * margin;
            float maxH = canvasSize.Height - 2 * margin;
            float canvasRatio = maxW / maxH;
            float w = canvasRatio > targetRatio
                      ? maxH * targetRatio
                      : maxW;
            float h = w / targetRatio;
            float x = margin + (maxW - w) / 2;
            float y = margin + (maxH - h) / 2;
            return new SKRect(x, y, x + w, y + h);
        }

        // 2) Отрисовать фон + рамки листа
        public static void DrawPageFrame(
            this SKCanvas canvas, SKImageInfo info, SKRect sheetRect)
        {
            // градиент
            using var bg = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(info.Width * 0.2f, info.Height * 0.2f),
                    new[] { SKColors.LightSkyBlue, SKColors.White },
                    null,
                    SKShaderTileMode.Clamp)
            };
            canvas.Save();
            canvas.ClipRect(sheetRect, SKClipOperation.Difference);
            canvas.DrawRect(-info.Width, -info.Height, info.Width * 2, info.Height * 2, bg);
            canvas.Restore();

            // рамка ГОСТ
            using var frame = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsAntialias = true
            };
            canvas.DrawRect(sheetRect, frame);
        }

        // 3) Собрать матрицу вписывания «мир→прямоугольник»
        public static SKMatrix ComposeWorldToArea(
            SKRect area,
            double minX, double maxX,
            double minY, double maxY)
        {
            float spanX = (float)(maxX - minX);
            float spanY = (float)(maxY - minY);

            var scale = SKMatrix.CreateScale(area.Width / spanX, -area.Height / spanY);
            var shiftWorld = SKMatrix.CreateTranslation((float)-minX, (float)-minY);
            var shiftArea = SKMatrix.CreateTranslation(area.Left, area.Bottom);

            return SKMatrix.Concat(shiftArea,
                   SKMatrix.Concat(scale, shiftWorld));
        }
    }
}
