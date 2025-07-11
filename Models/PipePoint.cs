using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PipeProfileAppMaui.Models
{
    public class PipePoint : INotifyPropertyChanged, IDataErrorInfo
    {
        private double _distance;
        private double _depth;
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

        public double Depth
        {
            get => _depth;
            set { if (_depth != value) { _depth = value; OnPropertyChanged(); } }
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
                    case nameof(Depth):
                        if (Depth < 0 || Depth > 100) return "Depth must be between 0 and 100.";
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