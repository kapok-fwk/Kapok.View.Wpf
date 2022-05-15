using System.Windows.Input;

namespace Kapok.View.Wpf;

public class ListControlEntryMouseDoubleClickCommand : RelayCommand<object>
{
    public static ListControlEntryMouseDoubleClickCommand BuildNew<TEntry>(IListPage<TEntry> page)
        where TEntry : class, new()
    {
        return new ListControlEntryMouseDoubleClickCommand(o =>
        {
            if (o is MouseButtonEventArgs mouseButtonEventArgs)
                ExecuteCommand(page, mouseButtonEventArgs);
            else if (o is TEntry entry)
                ExecuteCommand(page, entry);
        });
    }

    public static ListControlEntryMouseDoubleClickCommand BuildNew<TEntry, TSubEntry>(IDetailListPage<TEntry, TSubEntry> page)
        where TEntry : class, new()
        where TSubEntry : class, new()
    {
        return new ListControlEntryMouseDoubleClickCommand(o =>
        {
            if (o is MouseButtonEventArgs mouseButtonEventArgs)
                ExecuteCommand(page, mouseButtonEventArgs);
            else if (o is TSubEntry entry)
                ExecuteCommand(page, entry);
        });
    }

    private ListControlEntryMouseDoubleClickCommand(Action<object> execute) : base(execute)
    {
    }

    private static void ExecuteCommand<TEntry>(IListPage<TEntry> page, MouseButtonEventArgs eventArgs)
        where TEntry : class, new()
    {
        TEntry entry = (eventArgs.Source as System.Windows.Controls.DataGridRow)?.DataContext as TEntry;

        ExecuteCommand(page, entry);
    }

    private static void ExecuteCommand<TEntry>(IListPage<TEntry> page, TEntry entry)
        where TEntry : class, new()
    {
        if (entry == null)
            return;

        if (!page.IsEditable)
        {
            // open card page
            page.OpenCardPageAction?.Execute(new[] { entry }.ToList());
        }
    }

    private static void ExecuteCommand<TEntry, TSubEntry>(IDetailListPage<TEntry, TSubEntry> page, MouseButtonEventArgs eventArgs)
        where TEntry : class, new()
        where TSubEntry : class, new()
    {
        TSubEntry entry = (eventArgs.Source as System.Windows.Controls.DataGridRow)?.DataContext as TSubEntry;

        ExecuteCommand(page, entry);
    }

    private static void ExecuteCommand<TEntry, TSubEntry>(IDetailListPage<TEntry, TSubEntry> page, TSubEntry entry)
        where TEntry : class, new()
        where TSubEntry : class, new()
    {
        if (entry == null)
            return;

        if (!page.IsEditable)
        {
            // open card page
            page.OpenCardPageAction?.Execute(new[] { entry }.ToList());
        }
    }
}