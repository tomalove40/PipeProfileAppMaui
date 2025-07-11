using SkiaSharp;
using System.Collections.ObjectModel;
using PipeProfileAppMaui.Models;
using SkiaSharp.Views.Maui.Controls;

namespace PipeProfileAppMaui.Services
{
    public interface IPipeRenderer
    {
        /// <summary>
        /// Рисует профиль внутри area. ground/pipe передаются из VM.
        /// </summary>
        void Render(
          SKCanvas canvas,
          SKRect profileArea,
          IReadOnlyList<GroundPoint> groundPoints,
          IReadOnlyList<PipePoint> pipePoints,
          Func<double, double> interpolateGroundElevation);
    }
}