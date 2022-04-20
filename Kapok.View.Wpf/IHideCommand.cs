using System.ComponentModel;
using System.Windows.Input;

namespace Kapok.View.Wpf;

public interface IHideCommand : ICommand, INotifyPropertyChanged
{
    bool IsVisible { get; set; }
}