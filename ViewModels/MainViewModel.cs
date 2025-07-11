// File: Controls/MainViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PipeProfileAppMaui.Helpers;
using PipeProfileAppMaui.Models;
using PipeProfileAppMaui.Transforms;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace PipeProfileAppMaui.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // 1) Сырая матрица строк (11 строк по 12 столбцов)
        static readonly string[][] _initialStampText = new[]
        {
            new[]{ "",   "",   "",   "",   ".",  "",     "",     "",     "",     "",     "",     ""    },
            new[]{ "",   "",   "",   "",   "",    "",     "",     "",     "",     "",     "",     ""    },
            new[]{ "",   "",   "",   "",   "",    "",     "",     "",     "",     "",     "",     ""    },
            new[]{ "",   "",   "",   "",   "",    "",     "",     "",     "",     "",     "",     ""    },
            new[]{ "Изм.","Лист","№ Документ","Подпись","Дата","", "",    "",     "",     "",     "",     "" },
            new[]{ "Исп.            ","",   "",   "",   "",    "",     "Стадия","",    "",     "Лист","",     "Листов" },
            new[]{ "Пров.          ","",  "",   "",   "",    "",     "",     "",     "",     "1",    "",     "1"   },
            new[]{ "Пров.          ","",  "",   "",   "",    "",     "",     "",     "",     "",     "",     ""    },
            new[]{ "Пров.          ","",  "",   "",   "",    "",     "",     "",     "",     "",     "",     ""    },
            new[]{ "",   "",   "",   "",   "",    "",     "",     "",     "",     "",     "",     ""    },
            new[]{ "",   "",   "",   "",   "",    "",     "",     "",     "",     "",     "",     ""    },
        };

        [ObservableProperty]
        string screenOffsetRatioDisplay = "—";

        public void UpdatePipeOffsetRatio()
        {
            // ваша логика вычисления отношения
            // например:
            float sheetPx = 12;
            float firstOffset = 4;
            if (firstOffset <= 0)
                ScreenOffsetRatioDisplay = "∞";
            else
                ScreenOffsetRatioDisplay = (sheetPx / firstOffset).ToString("F2");
        }
        public ObservableCollection<ObservableCollection<StampCell>> StampCells { get; }
        public ViewTransform Transform { get; } = new ViewTransform();

        // Списки точек и флаги UI
        [ObservableProperty]
        private ObservableCollection<GroundPoint> groundPoints = new();

        [ObservableProperty]
        private ObservableCollection<PipePoint> pipePoints = new();

        [ObservableProperty]
        private bool showPointNumbers;

        [ObservableProperty]
        private float zoomLevel = 1f;

        [ObservableProperty]
        private float panX;

        [ObservableProperty]
        private float panY;

        public MainViewModel()
        {
            StampCells = new ObservableCollection<ObservableCollection<StampCell>>(
            _initialStampText
                .Select(row =>
                    new ObservableCollection<StampCell>(
                        row.Select(text => new StampCell { Text = text })
                    )
                )
            );
            // Пример начальных данных
            groundPoints.Add(new GroundPoint { Distance = 0, Elevation = 100 });
            groundPoints.Add(new GroundPoint { Distance = 50, Elevation = 200 });
            groundPoints.Add(new GroundPoint { Distance = 100, Elevation = 500});

            pipePoints.Add(new PipePoint { Distance = 0, Depth = 150 });
            pipePoints.Add(new PipePoint { Distance = 50, Depth = 270 });
            pipePoints.Add(new PipePoint { Distance = 100, Depth = 390 });
        }

        // Toggle отрисовки номеров точек
        [RelayCommand]
        void ToggleShowPointNumbers()
            => ShowPointNumbers = !ShowPointNumbers;

        // Сохранить в файл
        [RelayCommand]
        public async Task SaveToFileAsync()
        {
            var data = new ProfileData
            {
                GroundPoints = GroundPoints,
                PipePoints = PipePoints
            };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(FileSystem.AppDataDirectory, "profile.json");
            await File.WriteAllTextAsync(path, json);
        }

        // Сохранить как pdf
        [RelayCommand]
        public async Task SavePdfAsync()
        {
            var file = Path.Combine(FileSystem.AppDataDirectory, "sheet.pdf");
            using var doc = SKDocument.CreatePdf(file);
            var canvas = doc.BeginPage(595, 842); // A4 в 72 DPI
            Draw(canvas, new SKImageInfo(595, 842));
            doc.EndPage();
            doc.Close();

            await Shell.Current.DisplayAlert("PDF", $"Сохранено в:\n{file}", "OK");
        }

        // Загрузить из файла
        [RelayCommand]
        public async Task LoadFromFileAsync()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "profile.json");
            if (!File.Exists(path)) return;

            var json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<ProfileData>(json);
            if (data == null) return;

            GroundPoints.Clear();
            foreach (var gp in data.GroundPoints) GroundPoints.Add(gp);

            PipePoints.Clear();
            foreach (var pp in data.PipePoints) PipePoints.Add(pp);
        }

        // --- GroundPoint commands ---
        [RelayCommand]
        void AddGroundPoint()
        {
            GroundPoints.Add(new GroundPoint { Distance = 0, Elevation = 0 });
            RenumberGround();
        }

        [RelayCommand]
        void RemoveGroundPoint(GroundPoint p)
        {
            if (p == null) return;
            GroundPoints.Remove(p);
            RenumberGround();
        }

        [RelayCommand]
        void IncreaseGroundDistance(GroundPoint p)
        {
            if (p != null) p.Distance += 1;
        }

        [RelayCommand]
        void DecreaseGroundDistance(GroundPoint p)
        {
            if (p != null && p.Distance > 0) p.Distance -= 1;
        }

        void RenumberGround()
        {
            for (int i = 0; i < GroundPoints.Count; i++)
                GroundPoints[i].Index = i + 1;
        }

        // --- PipePoint commands ---
        [RelayCommand]
        void AddPipePoint()
        {
            PipePoints.Add(new PipePoint { Distance = 0, Depth = 0 });
            RenumberPipe();
        }

        [RelayCommand]
        void RemovePipePoint(PipePoint p)
        {
            if (p == null) return;
            PipePoints.Remove(p);
            RenumberPipe();
        }

        [RelayCommand]
        void IncreasePipeDistance(PipePoint p)
        {
            if (p != null) p.Distance += 1;
        }

        [RelayCommand]
        void DecreasePipeDistance(PipePoint p)
        {
            if (p != null && p.Distance > 0) p.Distance -= 1;
        }

        void RenumberPipe()
        {
            for (int i = 0; i < PipePoints.Count; i++)
                PipePoints[i].Index = i + 1;
        }

        // Внутренний класс для сериализации
        private class ProfileData
        {
            public ObservableCollection<GroundPoint> GroundPoints { get; set; } = new();
            public ObservableCollection<PipePoint> PipePoints { get; set; } = new();
        }
        /// <summary>
        /// Рисует профиль: применяем Pan/Zoom и отрисовываем базовую сетку + линии.
        /// </summary>
        public void Draw(SKCanvas canvas, SKImageInfo info)
        {
            // 1) Очистка и установка трансформации
            //canvas.Clear(SKColors.White);
            canvas.Save();
            //canvas.SetMatrix(Transform.Matrix);

            // 2) Опорная сетка
            using var gridPaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1f };
            for (int x = 0; x <= info.Width; x += 50)
                canvas.DrawLine(x, 0, x, info.Height, gridPaint);
            for (int y = 0; y <= info.Height; y += 50)
                canvas.DrawLine(0, y, info.Width, y, gridPaint);

            // 2.1) Подписи к сетке
            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 12 / Transform.Zoom,
                IsAntialias = true
            };
            // подписи по X (внизу)
            for (int x = 0; x <= info.Width; x += 50)
            {
                // чуть ниже нуля по Y
                canvas.DrawText(x.ToString(),
                                x + 2,    // небольшой отступ вправо
                                -4 / Transform.Zoom,
                                textPaint);
            }
            // подписи по Y (слева)
            for (int y = 0; y <= info.Height; y += 50)
            {
                // чуть левее нуля по X
                canvas.DrawText(y.ToString(),
                                -30 / Transform.Zoom,
                                y + 4,    // чуть ниже линии
                                textPaint);
            }

            // 3) Ломанная линия грунта
            if (GroundPoints.Count >= 2)
            {
                using var groundPaint = new SKPaint { Color = SKColors.Brown, StrokeWidth = 3, IsStroke = true };
                var path = new SKPath();
                for (int i = 0; i < GroundPoints.Count; i++)
                {
                    float x = (float)GroundPoints[i].Distance;
                    float y = (float)GroundPoints[i].Elevation;
                    if (i == 0) path.MoveTo(x, y);
                    else path.LineTo(x, y);
                }
                canvas.DrawPath(path, groundPaint);
            }

            // 4) Профиль трубы (глубина от поверхности)
            if (PipePoints.Count >= 2)
            {
                using var pipePaint = new SKPaint { Color = SKColors.Blue, StrokeWidth = 3, IsStroke = true };
                for (int i = 0; i < PipePoints.Count - 1; i++)
                {
                    var a = PipePoints[i];
                    var b = PipePoints[i + 1];

                    float gA = (float)InterpolateGroundElevation(a.Distance);
                    float gB = (float)InterpolateGroundElevation(b.Distance);

                    float yA = gA - (float)a.Depth;
                    float yB = gB - (float)b.Depth;

                    canvas.DrawLine(
                        (float)a.Distance, yA,
                        (float)b.Distance, yB,
                        pipePaint);
                }
            }

            canvas.Restore();
        }

        // Вспомогательный метод для линейной интерполяции уровня грунта
        public double InterpolateGroundElevation(double distance)
        {
            if (GroundPoints.Count == 0)
                return 0f;
            if (distance <= GroundPoints[0].Distance)
                return (float)GroundPoints[0].Elevation;
            if (distance >= GroundPoints[^1].Distance)
                return (float)GroundPoints[^1].Elevation;

            for (int i = 0; i < GroundPoints.Count - 1; i++)
            {
                var p0 = GroundPoints[i];
                var p1 = GroundPoints[i + 1];
                if (distance >= p0.Distance && distance <= p1.Distance)
                {
                    float t = (float)((distance - p0.Distance) / (p1.Distance - p0.Distance));
                    return (1 - t) * (float)p0.Elevation + t * (float)p1.Elevation;
                }
            }
            return (double)GroundPoints[^1].Elevation;
        }
    }
}