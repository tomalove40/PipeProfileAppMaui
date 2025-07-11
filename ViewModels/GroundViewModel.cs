using CommunityToolkit.Mvvm.ComponentModel;
using PipeProfileAppMaui.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PipeProfileAppMaui.ViewModels
{
    public partial class GroundViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<GroundPoint> _groundPoints;

        public GroundViewModel()
        {
            try
            {
                _groundPoints = new ObservableCollection<GroundPoint>();
                InitializeDefaultGroundPoints();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GroundViewModel Constructor Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void InitializeDefaultGroundPoints()
        {
            _groundPoints.Add(new GroundPoint { Distance = 0, Elevation = 100 });
            _groundPoints.Add(new GroundPoint { Distance = 50, Elevation = 110 });
            _groundPoints.Add(new GroundPoint { Distance = 100, Elevation = 105 });
            _groundPoints.Add(new GroundPoint { Distance = 150, Elevation = 115 });
            _groundPoints.Add(new GroundPoint { Distance = 200, Elevation = 100 });
        }
    }
}