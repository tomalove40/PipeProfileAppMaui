using System.Collections.ObjectModel;
using System.Windows.Input;                 // ICommand
using Microsoft.Maui.Controls;
using PipeProfileAppMaui.Models;

namespace PipeProfileAppMaui.Controls
{
    public partial class GroundPointsControl : ContentView
    {
        // 1) ��������� ����� � BindableProperty
        public static readonly BindableProperty GroundPointsProperty =
            BindableProperty.Create(
                nameof(GroundPoints),
                typeof(ObservableCollection<GroundPoint>),
                typeof(GroundPointsControl),
                new ObservableCollection<GroundPoint>());

        public ObservableCollection<GroundPoint> GroundPoints
        {
            get => (ObservableCollection<GroundPoint>)GetValue(GroundPointsProperty);
            set => SetValue(GroundPointsProperty, value);
        }

        // 2) ������� ��������� �����
        public static readonly BindableProperty AddPointCommandProperty =
            BindableProperty.Create(
                nameof(AddPointCommand),
                typeof(ICommand),
                typeof(GroundPointsControl));

        public ICommand AddPointCommand
        {
            get => (ICommand)GetValue(AddPointCommandProperty);
            set => SetValue(AddPointCommandProperty, value);
        }

        // 3) ������� �������� �����
        public static readonly BindableProperty RemovePointCommandProperty =
            BindableProperty.Create(
                nameof(RemovePointCommand),
                typeof(ICommand),
                typeof(GroundPointsControl));

        public ICommand RemovePointCommand
        {
            get => (ICommand)GetValue(RemovePointCommandProperty);
            set => SetValue(RemovePointCommandProperty, value);
        }

        // 4) ������� ���������� ����������
        public static readonly BindableProperty IncreaseDistanceCommandProperty =
            BindableProperty.Create(
                nameof(IncreaseDistanceCommand),
                typeof(ICommand),
                typeof(GroundPointsControl));

        public ICommand IncreaseDistanceCommand
        {
            get => (ICommand)GetValue(IncreaseDistanceCommandProperty);
            set => SetValue(IncreaseDistanceCommandProperty, value);
        }

        // 5) ������� ���������� ����������
        public static readonly BindableProperty DecreaseDistanceCommandProperty =
            BindableProperty.Create(
                nameof(DecreaseDistanceCommand),
                typeof(ICommand),
                typeof(GroundPointsControl));

        public ICommand DecreaseDistanceCommand
        {
            get => (ICommand)GetValue(DecreaseDistanceCommandProperty);
            set => SetValue(DecreaseDistanceCommandProperty, value);
        }

        public GroundPointsControl()
        {
            InitializeComponent();
        }
    }
}