using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kapok.View.Wpf;

public interface IImageCommand
{
    string ImageName { get; set; }

    string SmallImage { get; }

    string LargeImage { get; }
}

public class ImageCommand : RelayCommand, IImageCommand, IHideCommand
{
    private bool _isVisible = true;
    private string _imageName;

    public ImageCommand(Action execute)
        : base(execute)
    {
    }

    public ImageCommand(Action execute, Func<bool>? canExecute)
        : base(execute, canExecute)
    {
    }

    public ImageCommand(Action execute, string imageName)
        : base(execute)
    {
        ImageName = imageName;
    }

    public ImageCommand(Action execute, Func<bool>? canExecute, string imageName)
        : base(execute, canExecute)
    {
        ImageName = imageName;
    }

    public string ImageName
    {
        get => _imageName;
        set
        {
            if (_imageName == value) return;
            _imageName = value;
            SmallImage = ImageManager.GetImageResource(_imageName, ImageManager.ImageSize.Small);
            LargeImage = ImageManager.GetImageResource(_imageName, ImageManager.ImageSize.Large);
        }
    }

    public string SmallImage { get; private set; }

    public string LargeImage { get; private set; }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    #region INotifyPropertyChanged

    /// <summary>
    /// Checks if a property already matches a desired value. Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="storage">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">
    /// Name of the property used to notify listeners. This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.
    /// </param>
    /// <returns>
    /// True if the value was changed, false if the existing value matched the
    /// desired value.
    /// </returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
        
    #endregion
}

public class ImageCommand<T> : RelayCommand<T>, IImageCommand, IHideCommand
{
    private bool _isVisible = true;
    private string _imageName;

    public ImageCommand(Action<T?> execute)
        : base(execute)
    {
    }

    public ImageCommand(Action<T?> execute, Predicate<T?>? canExecute)
        : base(execute, canExecute)
    {
    }

    public ImageCommand(Action<T?> execute, string imageName)
        : base(execute)
    {
        ImageName = imageName;
    }

    public ImageCommand(Action<T?> execute, Predicate<T?>? canExecute, string imageName)
        : base(execute, canExecute)
    {
        ImageName = imageName;
    }

    public string ImageName
    {
        get => _imageName;
        set
        {
            if (_imageName == value) return;
            _imageName = value;
            SmallImage = ImageManager.GetImageResource(_imageName, ImageManager.ImageSize.Small);
            LargeImage = ImageManager.GetImageResource(_imageName, ImageManager.ImageSize.Large);
        }
    }

    public string SmallImage { get; set; }

    public string LargeImage { get; set; }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    #region INotifyPropertyChanged

    /// <summary>
    /// Checks if a property already matches a desired value. Sets the property and
    /// notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="storage">Reference to a property with both getter and setter.</param>
    /// <param name="value">Desired value for the property.</param>
    /// <param name="propertyName">
    /// Name of the property used to notify listeners. This
    /// value is optional and can be provided automatically when invoked from compilers that
    /// support CallerMemberName.
    /// </param>
    /// <returns>
    /// True if the value was changed, false if the existing value matched the
    /// desired value.
    /// </returns>
#pragma warning disable 693
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
#pragma warning restore 693
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
        
    #endregion
}