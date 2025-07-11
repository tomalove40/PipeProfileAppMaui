// Transforms/ViewTransform.cs
using SkiaSharp;

namespace PipeProfileAppMaui.Transforms
{
    public class ViewTransform
    {
        public float PanX { get; set; }
        public float PanY { get; set; }
        public float Zoom { get; set; } = 1f;

        /// <summary>
        /// Матрица Scale → Translate: p_screen = Zoom * p_world + Pan
        /// </summary>
        public SKMatrix Matrix
        {
            get
            {
                var scale = SKMatrix.CreateScale(Zoom, Zoom);
                var trans = SKMatrix.CreateTranslation(PanX, PanY);
                // сначала Scale, потом Translate
                return scale.PostConcat(trans);
            }
        }

        public SKMatrix InverseMatrix
        {
            get
            {
                if (Matrix.TryInvert(out var inv))
                    return inv;
                return SKMatrix.CreateIdentity();
            }
        }

        public void ZoomAt(float factor, SKPoint screenPoint)
        {
            var before = InverseMatrix.MapPoint(screenPoint);
            Zoom *= factor;
            var after = InverseMatrix.MapPoint(screenPoint);
            PanX += (after.X - before.X) * Zoom;
            PanY += (after.Y - before.Y) * Zoom;
        }
    }
}