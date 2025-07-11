using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using PipeProfileAppMaui.Models;

namespace PipeProfileAppMaui.Controls
{
    public partial class PipePointsControl : ContentView
    {
        // 1) ��������� ����� �����
        public static readonly BindableProperty PipePointsProperty =
            BindableProperty.Create(
                nameof(PipePoints),
                typeof(ObservableCollection<PipePoint>),
                typeof(PipePointsControl),
                new ObservableCollection<PipePoint>());

        public ObservableCollection<PipePoint> PipePoints
        {
            get => (ObservableCollection<PipePoint>)GetValue(PipePointsProperty);
            set => SetValue(PipePointsProperty, value);
        }

        // 2) ������� ��������� �����
        public static readonly BindableProperty AddPointCommandProperty =
            BindableProperty.Create(
                nameof(AddPointCommand),
                typeof(ICommand),
                typeof(PipePointsControl));

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
                typeof(PipePointsControl));

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
                typeof(PipePointsControl));

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
                typeof(PipePointsControl));

        public ICommand DecreaseDistanceCommand
        {
            get => (ICommand)GetValue(DecreaseDistanceCommandProperty);
            set => SetValue(DecreaseDistanceCommandProperty, value);
        }

        public PipePointsControl()
        {
            InitializeComponent();
        }
    }
}