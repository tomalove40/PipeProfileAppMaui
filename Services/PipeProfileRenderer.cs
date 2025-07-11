using SkiaSharp;
using PipeProfileAppMaui.ViewModels;
using PipeProfileAppMaui.Models;
using System.Collections.ObjectModel;
using SkiaSharp.Views.Maui.Controls;

namespace PipeProfileAppMaui.Services
{
    public class PipeProfileRenderer
    {
        private MainViewModel _viewModel;

        // Параметры трансформации
        public float Zoom { get; set; } = 1f;
        public float PanX { get; set; } = 0f;
        public float PanY { get; set; } = 0f;
        public bool ShowPointNumbers { get; set; } = false;

        // Конструктор c опциональным viewModel
        public PipeProfileRenderer(
            SKCanvas canvas,
            SKRect profileRect,                      // область на экране (px)
            IReadOnlyList<GroundPoint> groundPoints, // в мм
            IReadOnlyList<PipePoint> pipePoints,     // в мм
            Func<double, double> interpElevation)
        {
            if (groundPoints.Count < 2 || pipePoints.Count < 1)
                return;

            // 1) Находим мир-диапазоны по X (rast) и по Y (от minElevation–minDepth до maxElevation)
            float minX = (float)groundPoints.Min(p => p.Distance);
            float maxX = (float)groundPoints.Max(p => p.Distance);
            float spanX = maxX - minX;

            // по Y: берем уровень грунта и трубу, чтобы вместить обе линии
            float minGround = (float)groundPoints.Min(p => p.Elevation);
            float maxGround = (float)groundPoints.Max(p => p.Elevation);
            float minPipe = pipePoints
                              .Select(pp => (float)(interpElevation(pp.Distance) - pp.Depth))
                              .Min();
            float maxPipe = pipePoints
                              .Select(pp => (float)(interpElevation(pp.Distance) - pp.Depth))
                              .Max();

            float minY = Math.Min(minGround, minPipe);
            float maxY = Math.Max(maxGround, maxPipe);
            float spanY = maxY - minY;

            if (spanX <= 0 || spanY <= 0)
                return;

            // 2) Вычисляем шкалу «мм → пиксели» одинаково по обеим осям,
            //    чтобы профиль не искажался.
            float scaleX = profileRect.Width / spanX;
            float scaleY = profileRect.Height / spanY;
            float scale = Math.Min(scaleX, scaleY);

            // 3) Сдвиги: (distance–minX)*scale + profileRect.Left
            //    Y: (elevation–minY)*scale → Y-пиксели от низа области
            float ox = profileRect.Left;
            float oy = profileRect.Bottom;

            // 4) Клипуем рисунок строго профильно
            canvas.Save();
            canvas.ClipRect(profileRect);

            // 5) Рисуем грунт
            using var paintG = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Brown,
                StrokeWidth = 2,
                IsAntialias = true
            };
            var pathG = new SKPath();
            for (int i = 0; i < groundPoints.Count; i++)
            {
                float x = ox + (float)(groundPoints[i].Distance - minX) * scale;
                float y = oy - ((float)groundPoints[i].Elevation - minY) * scale;
                if (i == 0) pathG.MoveTo(x, y);
                else pathG.LineTo(x, y);
            }
            canvas.DrawPath(pathG, paintG);

            // 6) Рисуем трубу
            using var paintP = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 3,
                IsAntialias = true
            };
            var pathP = new SKPath();
            for (int i = 0; i < pipePoints.Count; i++)
            {
                float dist = (float)pipePoints[i].Distance;
                float elev = (float)(interpElevation(dist) - pipePoints[i].Depth);
                float x = ox + (dist - minX) * scale;
                float y = oy - (elev - minY) * scale;
                if (i == 0) pathP.MoveTo(x, y);
                else pathP.LineTo(x, y);
            }
            canvas.DrawPath(pathP, paintP);

            canvas.Restore();
        }

        // Позволяет передать или обновить ViewModel
        public void SetViewModel(MainViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Draw(
            SKCanvas canvas,
            int width,
            int height,
            ObservableCollection<GroundPoint> groundPoints,
            ObservableCollection<PipePoint> pipePoints,
            float zoomLevel,
            SKPoint panOffset)
        {
            if (_viewModel == null) return;

            canvas.Clear(SKColors.White);
            canvas.Save();

            // Трансформация
            canvas.Translate(PanX, PanY);
            canvas.Scale(Zoom);

            // Рисуем грунт (ломаная)
            if (_viewModel.GroundPoints.Count >= 2)
            {
                using var paintG = new SKPaint { Color = SKColors.Brown, StrokeWidth = 3, IsStroke = true };
                var pathG = new SKPath();
                for (int i = 0; i < _viewModel.GroundPoints.Count; i++)
                {
                    var p = _viewModel.GroundPoints[i];
                    float x = (float)(p.Distance * 5);
                    float y = (float)(height - p.Elevation * 2);
                    if (i == 0) pathG.MoveTo(x, y);
                    else pathG.LineTo(x, y);
                }
                canvas.DrawPath(pathG, paintG);
            }

            // Рисуем трубу (кривые Безье)
            if (_viewModel.PipePoints.Count >= 3)
            {
                var screenPoints = new List<SKPoint>();
                foreach (var p in _viewModel.PipePoints)
                {
                    float x = (float)(p.Distance * 5);
                    float groundElev = InterpolateGroundElevation(p.Distance);
                    float y = (float)(height - (groundElev - p.Depth) * 2);
                    screenPoints.Add(new SKPoint(x, y));
                }

                using var paintP = new SKPaint { Color = SKColors.Blue, StrokeWidth = 4, IsStroke = true, IsAntialias = true };
                var pathP = new SKPath();
                pathP.MoveTo(screenPoints[0]);

                for (int i = 0; i < screenPoints.Count - 1; i++)
                {
                    var p1 = screenPoints[i];
                    var p2 = screenPoints[i + 1];
                    var c1 = new SKPoint((p1.X + p2.X) / 2, p1.Y);
                    var c2 = new SKPoint((p1.X + p2.X) / 2, p2.Y);
                    pathP.CubicTo(c1, c2, p2);
                }
                canvas.DrawPath(pathP, paintP);

                // Касательные каждые 30px
                var measure = new SKPathMeasure(pathP, false);
                float len = measure.Length;
                var tanPaint = new SKPaint { Color = SKColors.Red, StrokeWidth = 2 };
                var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12 / Zoom, IsAntialias = true };

                for (float d = 0; d < len; d += 30)
                {
                    if (measure.GetPositionAndTangent(d, out var pos, out var tan))
                    {
                        float l = SKPoint.Distance(new SKPoint(0, 0), tan);
                        var unit = new SKPoint(tan.X / l, tan.Y / l);
                        var end = new SKPoint(pos.X + unit.X * 10, pos.Y + unit.Y * 10);
                        canvas.DrawLine(pos, end, tanPaint);

                        double distanceAt = pos.X / 5f;
                        float groundElev = InterpolateGroundElevation(distanceAt);
                        float pipeElev = (height - pos.Y) / 2f;
                        float depthAt = groundElev - pipeElev;

                        var angleTan = unit.Y / unit.X;
                        canvas.DrawText($"tan={angleTan:F2}", end.X + 5, end.Y - 5, textPaint);
                        canvas.DrawText($"глуб={depthAt:F2}", end.X + 5, end.Y + 10, textPaint);
                    }
                }
            }

            // Номера точек
            if (ShowPointNumbers)
            {
                using var numPaint = new SKPaint { Color = SKColors.Black, TextSize = 14 / Zoom, IsAntialias = true };
                for (int i = 0; i < _viewModel.PipePoints.Count; i++)
                {
                    var p = _viewModel.PipePoints[i];
                    float x = (float)(p.Distance * 5);
                    float groundElev = InterpolateGroundElevation(p.Distance);
                    float y = (float)(height - (groundElev - p.Depth) * 2);
                    canvas.DrawText((i + 1).ToString(), x + 5 / Zoom, y - 5 / Zoom, numPaint);
                }
                for (int i = 0; i < _viewModel.GroundPoints.Count; i++)
                {
                    var gp = _viewModel.GroundPoints[i];
                    float x = (float)(gp.Distance * 5);
                    float y = (float)(canvas.TotalMatrix.ScaleY * (height - gp.Elevation * 2));
                    canvas.DrawText((i + 1).ToString(), x + (5 / Zoom), y - 5 / Zoom, numPaint);
                }
            }

            canvas.Restore();
        }

        private float InterpolateGroundElevation(double distance)
        {
            var pts = _viewModel.GroundPoints;
            if (pts.Count == 0) return 0;
            if (distance <= pts[0].Distance) return (float)pts[0].Elevation;
            if (distance >= pts[^1].Distance) return (float)pts[^1].Elevation;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                if (distance >= pts[i].Distance && distance <= pts[i + 1].Distance)
                {
                    float d0 = (float)pts[i].Distance;
                    float d1 = (float)pts[i + 1].Distance;
                    float e0 = (float)pts[i].Elevation;
                    float e1 = (float)pts[i + 1].Elevation;
                    float t = (float)((distance - d0) / (d1 - d0));
                    return e0 + t * (e1 - e0);
                }
            }
            return (float)pts[^1].Elevation;
        }
    }
}