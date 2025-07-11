using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PipeProfileAppMaui.Models
{
    public class GroundPoint : INotifyPropertyChanged, IDataErrorInfo
    {
        private double _distance;
        private double _elevation;
        private bool _isHighlighted;
        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Index)));
                }
            }
        }

        public double Distance
        {
            get => _distance;
            set { if (_distance != value) { _distance = value; OnPropertyChanged(); } }
        }

        public double Elevation
        {
            get => _elevation;
            set { if (_elevation != value) { _elevation = value; OnPropertyChanged(); } }
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set { if (_isHighlighted != value) { _isHighlighted = value; OnPropertyChanged(); } }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Distance):
                        if (Distance < 0) return "Distance must be non-negative.";
                        break;
                    case nameof(Elevation):
                        if (Elevation < -1000 || Elevation > 10000) return "Elevation must be between -1000 and 10000.";
                        break;
                }
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}