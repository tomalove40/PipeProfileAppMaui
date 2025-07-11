// в ViewModels/RibbonItems.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace PipeProfileAppMaui.ViewModels
{
    public interface IRibbonItem { }

    public class RibbonButtonItem : IRibbonItem
    {
        public string Text { get; }
        public ICommand Command { get; }
        public RibbonButtonItem(string text, ICommand cmd)
        {
            Text = text; Command = cmd;
        }
    }

    public class RibbonLabelItem : IRibbonItem, INotifyPropertyChanged
    {
        string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public RibbonLabelItem(string txt) => _text = txt;
    }
    public class RibbonTab
    {
        public string Header { get; }
        public ObservableCollection<IRibbonItem> Items { get; }
        public RibbonTab(string header, IEnumerable<IRibbonItem> items)
        {
            Header = header;
            Items = new ObservableCollection<IRibbonItem>(items);
        }
    }
}