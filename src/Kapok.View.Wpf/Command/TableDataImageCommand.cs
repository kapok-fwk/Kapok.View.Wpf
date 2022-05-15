using System.Collections;
using System.Diagnostics;
using System.Windows.Input;

namespace Kapok.View.Wpf;

public interface ITableDataCommand : ICommand
{
}

public class TableDataImageCommand : ImageCommand<IList>, ITableDataCommand
{
    public TableDataImageCommand(Action<IList> execute) : base(execute)
    {
    }

    public TableDataImageCommand(Action<IList> execute, Predicate<IList> canExecute) : base(execute, canExecute)
    {
    }

    public TableDataImageCommand(Action<IList> execute, string imageName) : base(execute, imageName)
    {
    }

    public TableDataImageCommand(Action<IList> execute, Predicate<IList> canExecute, string imageName) : base(execute, canExecute, imageName)
    {
    }

    public override void Execute(object parameter)
    {
        if (parameter is IList selectedEntries)
        {
            // remove new item placeholder
            var newSelectedEntriesList = new List<object>(
                from e in selectedEntries.Cast<object>()
                where !(
                    e.GetType().FullName == "MS.Internal.NamedObject" &&
                    e.ToString() == "{NewItemPlaceholder}"
                )
                select e
            );

            if (newSelectedEntriesList.Count == 0)
            {
                Debug.WriteLine("Canceled TableDataImageCommand: No entry selected.");
                return;
            }

            parameter = newSelectedEntriesList;
        }

        base.Execute(parameter);
    }
}

public class TableDataImageCommand<TEntry> : TableDataImageCommand
{
    private static IList<TEntry> CastList(IList list)
    {
        if (list == null)
            return null;

        if (list is IList<TEntry> finalList)
            return finalList.ToList();

        // remove new item placeholder
        var newSelectedEntriesList = new List<TEntry>(
            from e in list.Cast<object>()
            where !(
                e.GetType().FullName == "MS.Internal.NamedObject" &&
                e.ToString() == "{NewItemPlaceholder}"
            )
            select (TEntry)e
        );

        if (newSelectedEntriesList.Count == 0)
        {
            return null;
        }

        return newSelectedEntriesList;
    }

    public TableDataImageCommand(Action<IList<TEntry>> execute)
        : base(
            l => execute(CastList(l))
        )
    {
    }

    public TableDataImageCommand(Action<IList<TEntry>> execute, Predicate<IList<TEntry>> canExecute)
        : base(
            l => execute(CastList(l)),
            l => canExecute(CastList(l))
        )
    {
    }

    public TableDataImageCommand(Action<IList<TEntry>> execute, string imageName)
        : base(
            l => execute(CastList(l)),
            imageName
        )
    {
    }

    public TableDataImageCommand(Action<IList<TEntry>> execute, Predicate<IList<TEntry>> canExecute, string imageName)
        : base(
            l => execute(CastList(l)),
            l => canExecute(CastList(l)),
            imageName
        )
    {
    }
}