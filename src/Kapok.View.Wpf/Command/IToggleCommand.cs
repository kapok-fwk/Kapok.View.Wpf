using System.ComponentModel;
using System.Windows.Input;

namespace Kapok.View.Wpf;

public interface IToggleCommand : ICommand, INotifyPropertyChanged
{
    bool IsChecked { get; set; }
}