using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PipeProfileAppMaui.ViewModels
{
    //public record RibbonButton(string Text, ICommand Command);

    //public record RibbonTab(string Header, IEnumerable<RibbonButton> Buttons);

    // ViewModels/RibbonViewModel.cs
    public partial class RibbonViewModel : ObservableObject
    {
        public ObservableCollection<RibbonTab> RibbonTabs { get; } = new();

        [ObservableProperty]
        RibbonTab _selectedRibbonTab;

        public RibbonViewModel(MainViewModel mainVm)
        {
            // 1) домашние кнопки
            var homeItems = new List<IRibbonItem>
    {
      new RibbonButtonItem("💾 PDF",    mainVm.SavePdfCommand),
      new RibbonButtonItem("📂 Загрузить", mainVm.LoadFromFileCommand),
      new RibbonButtonItem("🔢 Точки", mainVm.ToggleShowPointNumbersCommand),
      // 2) метка-лейбл для ratio
      new RibbonLabelItem(mainVm.ScreenOffsetRatioDisplay)
    };

            // 3) Подписываемся на изменение ratio в VM
            mainVm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.ScreenOffsetRatioDisplay))
                {
                    // последний элемент всегда наш лейбл
                    var lbl = (RibbonLabelItem)homeItems[^1];
                    lbl.Text = mainVm.ScreenOffsetRatioDisplay;
                }
            };

            // 4) вкладка «Главная»
            RibbonTabs.Add(new RibbonTab("Главная", homeItems));

            // 5) вкладка «Вставка»
            var insertItems = new IRibbonItem[]
            {
              new RibbonButtonItem("➕ Грунт", mainVm.AddGroundPointCommand),
              new RibbonButtonItem("➕ Труба", mainVm.AddPipePointCommand)
            };
            RibbonTabs.Add(new RibbonTab("Вставка", insertItems));

            SelectedRibbonTab = RibbonTabs[0];
        }

        [RelayCommand]
        void SelectTab(RibbonTab tab)
          => SelectedRibbonTab = tab;
    }
}