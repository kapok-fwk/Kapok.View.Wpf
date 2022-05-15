using System.ComponentModel;

namespace Kapok.View.Wpf;

public class ToggleImageCommand : ImageCommand, IToggleCommand, IHideCommand, INotifyPropertyChanged
{
    public ToggleImageCommand(Action execute, string imageName) : base(execute, imageName)
    {
    }

    public ToggleImageCommand(Action execute, Func<bool> canExecute, string imageName) : base(execute, canExecute, imageName)
    {
    }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}