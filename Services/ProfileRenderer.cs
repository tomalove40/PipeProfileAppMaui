using System;
using System.Collections.Generic;
using System.Linq;
using PipeProfileAppMaui.Models;
using SkiaSharp;

namespace PipeProfileAppMaui.Services
{
    /// <summary>
    /// Рисует «вписанный» профиль грунта и трубы
    /// в заданный прямоугольник экранных координат.
    /// Толщина линий остаётся постоянной в пикселях экрана.
    /// </summary>
    public class ProfileRenderer
    {
        /// <summary>
        /// Рисует профиль.
        /// </summary>
        /// <param name="canvas">Цель рисования</param>
        /// <param name="profileArea">Экранная область для профиля</param>
        /// <param name="groundPoints">Точки грунта</param>
        /// <param name="pipePoints">Точки трубы</param>
        /// <param name="interpolateGroundElevation">
        /// Функция линейной интерполяции высоты грунта по расстоянию
        /// </param>
        public void Render(
            SKCanvas canvas,
            SKRect profileArea,
            IReadOnlyList<GroundPoint> groundPoints,
            IReadOnlyList<PipePoint> pipePoints,
            Func<double, double> interpolateGroundElevation)
        {
            if (groundPoints.Count < 2 || pipePoints.Count < 1)
                return;

            // 1) Собираем исходные «мировые» пути
            var soilPath = new SKPath();
            for (int i = 0; i < groundPoints.Count; i++)
            {
                var p = groundPoints[i];
                var pt = new SKPoint((float)p.Distance, (float)p.Elevation);
                if (i == 0) soilPath.MoveTo(pt);
                else soilPath.LineTo(pt);
            }

            var pipePath = new SKPath();
            for (int i = 0; i < pipePoints.Count; i++)
            {
                var p = pipePoints[i];
                float x = (float)p.Distance;
                float y = (float)(interpolateGroundElevation(p.Distance) - p.Depth);
                if (i == 0) pipePath.MoveTo(x, y);
                else pipePath.LineTo(x, y);
            }

            // 2) Вычисляем диапазоны
            float minX = groundPoints.Min(p => (float)p.Distance);
            float maxX = groundPoints.Max(p => (float)p.Distance);
            float spanX = maxX - minX;

            var yG = groundPoints.Select(p => (float)p.Elevation);
            var yP = pipePoints.Select(p => (float)(interpolateGroundElevation(p.Distance) - p.Depth));
            float minY = Math.Min(yG.Min(), yP.Min());
            float maxY = Math.Max(yG.Max(), yP.Max());
            float spanY = maxY - minY;

            // 3) Строим единую матрицу world→device
            //   a) масштаб + переворот Y
            var scale = SKMatrix.CreateScale(
                profileArea.Width / spanX,
               -profileArea.Height / spanY);

            //   b) сдвиг в ноль мировых минусов
            var shiftWorld = SKMatrix.CreateTranslation(-minX, -minY);

            //   c) сдвиг в экранную область
            var shiftDev = SKMatrix.CreateTranslation(
                profileArea.Left, profileArea.Bottom);

            // M = shiftDev ∘ scale ∘ shiftWorld
            var M1 = SKMatrix.Concat(scale, shiftWorld);
            var M = SKMatrix.Concat(shiftDev, M1);

            // 4) Преобразуем пути в экранные
            var devSoil = new SKPath(); soilPath.Transform(M, devSoil);
            var devPipe = new SKPath(); pipePath.Transform(M, devPipe);

            // 5) Клипуем строго по области профиля
            canvas.Save();
            canvas.ClipRect(profileArea);

            // 6) Рисуем грунт с толщиной 2px экрана
            using (var paintG = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Brown,
                StrokeWidth = 2,
                IsAntialias = true
            })
            {
                canvas.DrawPath(devSoil, paintG);
            }

            // 7) Рисуем трубу с толщиной 3px экрана
            using (var paintP = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 3,
                IsAntialias = true
            })
            {
                canvas.DrawPath(devPipe, paintP);
            }

            canvas.Restore();
        }
        public void RenderInWorld(
          SKCanvas canvas,
          SKRect area,            // область в мм [left,bottom..right,top]
          IReadOnlyList<GroundPoint> ground,
          IReadOnlyList<PipePoint> pipe,
          Func<double, double> interpElevation)
        {
            if (ground.Count < 2 || pipe.Count < 1)
                return;

            // Ломанная грунта
            using var gp = new SKPaint { Color = SKColors.Brown, StrokeWidth = 2, IsAntialias = true };
            var pathG = new SKPath();
            for (int i = 0; i < ground.Count; i++)
            {
                var p = ground[i];
                float x = area.Left + (float)p.Distance;
                float y = area.Bottom + (float)p.Elevation;
                if (i == 0) pathG.MoveTo(x, y);
                else pathG.LineTo(x, y);
            }
            canvas.DrawPath(pathG, gp);

            // Труба
            using var pp = new SKPaint { Color = SKColors.Blue, StrokeWidth = 3, IsAntialias = true };
            var pathP = new SKPath();
            for (int i = 0; i < pipe.Count; i++)
            {
                var p = pipe[i];
                float x = area.Left + (float)p.Distance;
                float e = (float)(interpElevation(p.Distance) - p.Depth);
                float y = area.Bottom + e;
                if (i == 0) pathP.MoveTo(x, y);
                else pathP.LineTo(x, y);
            }
            canvas.DrawPath(pathP, pp);
        }
    }
}