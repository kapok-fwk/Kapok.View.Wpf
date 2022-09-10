using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Kapok.View.Wpf;

//[ValueConversion(typeof(IAction), typeof(ICommand))]
//[ValueConversion(typeof(IAction<>), typeof(ICommand))]
//[ValueConversion(typeof(IDataSetSelectionAction), typeof(ICommand))]
//[ValueConversion(typeof(IDataSetSelectionAction<>), typeof(ICommand))]
[Localizability(LocalizationCategory.NeverLocalize)]
// ReSharper disable once InconsistentNaming
public class IActionToICommandConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType != typeof(ICommand))
            throw new InvalidOperationException("Target type must be System.Windows.Input.ICommand.");
            
        if (value == null)
            return null;

        var type = value.GetType();

        if (type.IsAssignableToGenericType(typeof(IDataSetSelectionAction<>)))
        {
            var entryType = type.GenericTypeArguments[0];

            var genericType = typeof(IList<>).MakeGenericType(entryType);
            var actionInterfaceType = typeof(IAction<>).MakeGenericType(genericType);

            // ReSharper disable once PossibleNullReferenceException
            var image = actionInterfaceType.GetProperty(nameof(IAction.Image)).GetValue(value);

            var commandType = typeof(TableDataImageCommand<>).MakeGenericType(entryType);

            return ConstructGenericCommand(genericType, actionInterfaceType, commandType, value, (string?) image);
        }

        if (type.IsAssignableToGenericType(typeof(IAction<>)))
        {
            var genericType = type.GenericTypeArguments[0];

            var actionInterfaceType = typeof(IAction<>).MakeGenericType(genericType);

            // ReSharper disable once PossibleNullReferenceException
            var image = actionInterfaceType.GetProperty(nameof(IAction.Image)).GetValue(value);

            var commandType = image != null
                ? typeof(ImageCommand<>).MakeGenericType(genericType)
                : typeof(RelayCommand<>).MakeGenericType(genericType);

            return ConstructGenericCommand(genericType, actionInterfaceType, commandType, value, (string) image);
        }

        if (value is IDataSetSelectionAction dataSetSelectionAction)
        {
            var command = new TableDataImageCommand(dataSetSelectionAction.Execute, dataSetSelectionAction.CanExecute, dataSetSelectionAction.Image)
            {
                IsVisible = dataSetSelectionAction.IsVisible
            };

            RegisterPassCanExecuteChangeToCommand(dataSetSelectionAction, command);

            return command;
        }

        if (value is IToggleAction toggleAction)
        {
            var toggleCommand = new ToggleImageCommand(toggleAction.Execute, toggleAction.CanExecute, toggleAction.Image)
            {
                IsVisible = toggleAction.IsVisible,
                IsChecked = toggleAction.IsChecked
            };

            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(
                toggleAction, nameof(toggleAction.PropertyChanged),
                handler: (sender, args) => ToggleAction_PropertyChanged(sender, args, toggleCommand));
            WeakEventManager<ToggleImageCommand, PropertyChangedEventArgs>.AddHandler(
                toggleCommand, nameof(toggleCommand.PropertyChanged),
                handler: (sender, args) => ToggleCommand_PropertyChanged(sender, args, toggleAction));

            RegisterPassCanExecuteChangeToCommand(toggleAction, toggleCommand);

            return toggleCommand;
        }

        if (value is IAction action)
        {
            if (action.Image != null)
            {
                var command = new ImageCommand(action.Execute, action.CanExecute, action.Image)
                {
                    // TODO: here we just pass over the value once, but there is no binding so that when INotifyPropertyChanged is called, the IHideCommand.IsVisible value is updated.
                    IsVisible = action.IsVisible
                };
                RegisterPassCanExecuteChangeToCommand(action, command);
                return command;
            }
            else
            {
                // TODO: RelayCommand does not implement the 'IsVisible' property which is not passed over here!
                var command = new RelayCommand(action.Execute, action.CanExecute);
                RegisterPassCanExecuteChangeToCommand(action, command);
                return command;
            }
        }

        // Object without action, return null.
        return null;
    }

    private void RegisterPassCanExecuteChangeToCommand(IAction action, RelayCommand relayCommand)
    {
        WeakEventManager<IAction, EventArgs>.AddHandler(
            action, nameof(action.CanExecuteChanged),
            handler: (s, e) => relayCommand.RaiseCanExecuteChanged());
    }
    private void RegisterPassCanExecuteChangeToCommand<T>(IAction<T> action, RelayCommand<T> relayCommand)
    {
        WeakEventManager<IAction<T>, EventArgs>.AddHandler(
            action, nameof(action.CanExecuteChanged),
            handler: (s, e) => relayCommand.RaiseCanExecuteChanged());
    }
    private void RegisterPassCanExecuteChangeToCommand<T, TEntity>(IAction<T> action, TableDataImageCommand<TEntity> relayCommand)
    {
        WeakEventManager<IAction<T>, EventArgs>.AddHandler(
            action, nameof(action.CanExecuteChanged),
            handler: (s, e) => relayCommand.RaiseCanExecuteChanged());
    }

    private static bool _syncToggle;

    private void ToggleAction_PropertyChanged(object sender, PropertyChangedEventArgs e, IToggleCommand toggleCommand)
    {
        if (e.PropertyName != nameof(ToggleImageCommand.IsChecked)) return;
        if (_syncToggle) return;

        _syncToggle = true;

        toggleCommand.IsChecked = ((IToggleAction) sender).IsChecked;

        _syncToggle = false;
    }

    private void ToggleCommand_PropertyChanged(object sender, PropertyChangedEventArgs e, IToggleAction toggleAction)
    {
        if (e.PropertyName != nameof(ToggleImageCommand.IsChecked)) return;
        if (_syncToggle) return;

        _syncToggle = true;

        toggleAction.IsChecked = ((IToggleCommand) sender).IsChecked;

        _syncToggle = false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private object ConstructGenericCommand(Type genericType, Type actionInterfaceType, Type commandType, object baseAction, string? image)
    {
        var executeDelegateType = typeof(Action<>).MakeGenericType(genericType);
        var canExecuteDelegateType = typeof(Predicate<>).MakeGenericType(genericType);
        var executeMethodInfo = actionInterfaceType.GetMethod(nameof(IAction.Execute), BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);
        var canExecuteMethodInfo = actionInterfaceType.GetMethod(nameof(IAction.CanExecute), BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);
        // ReSharper disable once AssignNullToNotNullAttribute
        var executeDelegate = Delegate.CreateDelegate(executeDelegateType, baseAction, executeMethodInfo);
        // ReSharper disable once AssignNullToNotNullAttribute
        var canExecuteDelegate = Delegate.CreateDelegate(canExecuteDelegateType, baseAction, canExecuteMethodInfo);

        object newCommand;
        var constructor = commandType.GetConstructor(new []
        {
            executeDelegateType,
            canExecuteDelegateType,
            typeof(string)
        });
        if (constructor == null)
        {
            constructor = commandType.GetConstructor(new[]
            {
                executeDelegateType,
                canExecuteDelegateType
            });

            if (constructor == null)
                throw new NotSupportedException($"Could not found fitting constructor for {commandType} command.");

            newCommand = constructor.Invoke(new object[]
            {
                executeDelegate,
                canExecuteDelegate
            });
        }
        else
        {
            newCommand = constructor.Invoke(new object[]
            {
                executeDelegate,
                canExecuteDelegate,
                image
            });
        }

        if (typeof(IHideCommand).IsAssignableFrom(commandType))
        {
            // TODO: need here a synchronization via INotifyPropertyChanged

            // ReSharper disable once PossibleNullReferenceException
            var isVisibleValue = (bool)actionInterfaceType.GetProperty(nameof(IAction.IsVisible)).GetValue(baseAction);

            // ReSharper disable once PossibleNullReferenceException
            typeof(IHideCommand).GetProperty(nameof(IHideCommand.IsVisible)).SetValue(newCommand, isVisibleValue);
        }

        MethodInfo method;
        if (commandType.GetGenericTypeDefinition() == typeof(TableDataImageCommand<>))
        {
            // invoke RegisterPassCanExecuteChangeToCommand<genericType, genericType.GetGenericArguments()[0]>(baseAction, newCommand);
            method = typeof(IActionToICommandConverter)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == nameof(RegisterPassCanExecuteChangeToCommand) &&
                                     m.IsGenericMethod &&
                                     m.GetGenericArguments().Length == 2);

            method = method.MakeGenericMethod(genericType, genericType.GetGenericArguments()[0]);
        }
        else
        {
            // invoke RegisterPassCanExecuteChangeToCommand<genericType>(baseAction, newCommand);
            method = typeof(IActionToICommandConverter)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == nameof(RegisterPassCanExecuteChangeToCommand) &&
                                     m.IsGenericMethod &&
                                     m.GetGenericArguments().Length == 1);
                
            method = method.MakeGenericMethod(genericType);
        } 
        method.Invoke(this, new[]
        {
            baseAction,
            newCommand
        });

        return newCommand;
    }
}