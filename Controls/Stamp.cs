using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PipeProfileAppMaui.Models;
using SkiaSharp;

namespace PipeProfileAppMaui.Controls
{
    /// <summary>
    /// ГОСТ-штамп с объединёнными ячейками.
    /// </summary>
    public class Stamp
    {
        /// <summary>Ширины колонок в мм</summary>
        public float[] ColumnWidthsMm { get; }

        /// <summary>Высоты строк в мм</summary>
        public float[] RowHeightsMm { get; }

        /// <summary>Текст в каждой ячейке [row][col]</summary>
        public ObservableCollection<ObservableCollection<StampCell>> StampCells { get; set; }

        /// <summary>Списки объединений ячеек</summary>
        public List<CellSpan> Spans { get; } = new();

        public Stamp()
        {
            var columnWidthsMm = new float[] { 70, 100, 230, 150, 100, 700, 50, 50, 50, 50, 120, 180 };
            var rowHeightsMm = Enumerable.Repeat(5f, 11).ToArray();
            ColumnWidthsMm = columnWidthsMm;
            RowHeightsMm = rowHeightsMm;
            var initialCells = new ObservableCollection<ObservableCollection<StampCell>>();
            StampCells = initialCells
                             ?? throw new ArgumentNullException(nameof(initialCells));
            this.Spans.AddRange(new[]
            {
              new CellSpan(0, 5, 2, 7),  // Код проекта
              new CellSpan(2, 5, 3, 7),  // Наименование проекта
              new CellSpan(5, 0, 1, 2),  // Исп
              new CellSpan(6, 0, 1, 2),  // Пров. 1
              new CellSpan(7, 0, 1, 2),  // Пров. 2
              new CellSpan(8, 0, 1, 2),  // Пров. 3
              new CellSpan(9, 0, 1, 2),  // Пров. 4
              new CellSpan(10, 0, 1, 2),  // Пров. 5
              new CellSpan(5, 5, 3, 1),  // Труба
              new CellSpan(8, 5, 3, 1),  // Нижний левый блок
              new CellSpan(8, 6, 3, 6),  // Нижний правый блок
              new CellSpan(6, 6, 2, 3),  // Наим стадии
              new CellSpan(5, 6, 1, 3),  // Заголовок стадия
              new CellSpan(6, 9, 2, 2),  // Номер листа
              new CellSpan(5, 9, 1, 2),  // Заголовок лист
              new CellSpan(6, 11, 2, 1),  // Кол-во листов
            });
        }

        /// <summary>
        /// Рисует рамки ячеек и текст, учитывая объединения.
        /// </summary>
        public void Draw(SKCanvas canvas, SKRect area)
        {
            // мм→пиксели
            float totalW = ColumnWidthsMm.Sum();
            float totalH = RowHeightsMm.Sum();
            float pxX = area.Width / totalW;
            float pxY = area.Height / totalH;

            using var framePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsAntialias = true
            };

            // Шрифт: 4 мм строки
            float fontSize = pxY * 3f;
            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = fontSize,
                IsAntialias = true
            };
            var fm = textPaint.FontMetrics;
            float lineH = fm.Descent - fm.Ascent;

            // Пробегаем по всем клеткам
            float yAccum = area.Top;
            for (int r = 0; r < RowHeightsMm.Length; r++)
            {
                float rowH = RowHeightsMm[r] * pxY;
                float xAccum = area.Left;

                for (int c = 0; c < ColumnWidthsMm.Length; c++)
                {
                    // Проверяем, начало ли спана или «внутренняя» клетка
                    var cover = Spans.FirstOrDefault(s =>
                        s.Row <= r && r < s.Row + s.RowSpan &&
                        s.Col <= c && c < s.Col + s.ColSpan);

                    // если interior—пропустить
                    if (cover != null && (cover.Row != r || cover.Col != c))
                    {
                        xAccum += ColumnWidthsMm[c] * pxX;
                        continue;
                    }

                    // Размер спана
                    int row0 = cover?.Row ?? r;
                    int col0 = cover?.Col ?? c;
                    int rs = cover?.RowSpan ?? 1;
                    int cs = cover?.ColSpan ?? 1;

                    // Расчёт rect спана
                    float x0 = area.Left + ColumnWidthsMm.Take(col0).Sum() * pxX;
                    float y0 = area.Top + RowHeightsMm.Take(row0).Sum() * pxY;
                    float w = ColumnWidthsMm.Skip(col0).Take(cs).Sum() * pxX;
                    float h = RowHeightsMm.Skip(row0).Take(rs).Sum() * pxY;
                    var rect = new SKRect(x0, y0, x0 + w, y0 + h);

                    // 1) Рисуем рамку объединённой клетки
                    canvas.DrawRect(rect, framePaint);

                    // 2) Клиппинг → текст вписывается внутрь
                    canvas.Save();
                    canvas.ClipRect(rect);

                    string txt = StampCells[row0][col0].Text ?? "";
                    var lines = txt.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    float totalTextH = lines.Length * lineH;
                    float yText = rect.Top + (rect.Height - totalTextH) / 2 - fm.Ascent;

                    foreach (var line in lines)
                    {
                        float tw = textPaint.MeasureText(line);
                        float xText = rect.Left + (rect.Width - tw) / 2;
                        canvas.DrawText(line, xText, yText, textPaint);
                        yText += lineH;
                    }

                    canvas.Restore();

                    xAccum += ColumnWidthsMm[c] * pxX;
                }

                yAccum += rowH;
            }
        }        /// <summary>
                 /// Возвращает (row,col) ячейки по точке в «мировых» пикселях.
                 /// </summary>
        public (int row, int col)? HitTest(SKPoint worldPoint, SKRect area)
        {
            float totalW = ColumnWidthsMm.Sum();
            float totalH = RowHeightsMm.Sum();
            float pxX = area.Width / totalW;
            float pxY = area.Height / totalH;

            float dx = (worldPoint.X - area.Left) / pxX;
            float dy = (worldPoint.Y - area.Top) / pxY;

            float accY = 0;
            for (int r = 0; r < RowHeightsMm.Length; r++)
            {
                float rowH = RowHeightsMm[r];
                float accX = 0;
                for (int c = 0; c < ColumnWidthsMm.Length; c++)
                {
                    var span = Spans
                        .FirstOrDefault(s => s.Row == r && s.Col == c);
                    int rs = span?.RowSpan ?? 1;
                    int cs = span?.ColSpan ?? 1;
                    float cellW = ColumnWidthsMm.Skip(c).Take(cs).Sum();
                    float cellH = RowHeightsMm.Skip(r).Take(rs).Sum();

                    if (dx >= accX && dx < accX + cellW &&
                        dy >= accY && dy < accY + cellH)
                    {
                        return (r, c);
                    }

                    accX += ColumnWidthsMm[c];
                }
                accY += rowH;
            }
            return null;
        }

        /// <summary>
        /// Задаёт «мировую» область одной ячейки (с учётом span).
        /// </summary>
        public SKRect GetCellRect(int row, int col, SKRect area)
        {
            float totalW = ColumnWidthsMm.Sum();
            float totalH = RowHeightsMm.Sum();
            float pxX = area.Width / totalW;
            float pxY = area.Height / totalH;

            var span = Spans
                .FirstOrDefault(s =>
                    s.Row <= row && row < s.Row + s.RowSpan &&
                    s.Col <= col && col < s.Col + s.ColSpan);

            int row0 = span?.Row ?? row;
            int col0 = span?.Col ?? col;
            int rs = span?.RowSpan ?? 1;
            int cs = span?.ColSpan ?? 1;

            float x = area.Left + ColumnWidthsMm.Take(col0).Sum() * pxX;
            float y = area.Top + RowHeightsMm.Take(row0).Sum() * pxY;
            float w = ColumnWidthsMm.Skip(col0).Take(cs).Sum() * pxX;
            float h = RowHeightsMm.Skip(row0).Take(rs).Sum() * pxY;

            return new SKRect(x, y, x + w, y + h);
        }
    }

    /// <summary>Описывает объединённую область ячеек.</summary>
    public record CellSpan(int Row, int Col, int RowSpan, int ColSpan);
}