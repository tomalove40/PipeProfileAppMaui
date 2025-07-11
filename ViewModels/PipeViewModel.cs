using CommunityToolkit.Mvvm.ComponentModel;
using PipeProfileAppMaui.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PipeProfileAppMaui.ViewModels
{
    public partial class PipeViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PipePoint> _pipePoints;

        public PipeViewModel()
        {
            try
            {
                _pipePoints = new ObservableCollection<PipePoint>();
                InitializeDefaultPipePoints();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PipeViewModel Constructor Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void InitializeDefaultPipePoints()
        {
            _pipePoints.Add(new PipePoint { Distance = 0, Depth = 10 });
            _pipePoints.Add(new PipePoint { Distance = 100, Depth = 12 });
            _pipePoints.Add(new PipePoint { Distance = 200, Depth = 11 });
        }
    }
}