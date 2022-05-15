using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using Kapok.Core;

namespace Kapok.View.Wpf;

public class OpenPageCommand : IImageCommand, ITableDataCommand
{
    public readonly Type PageType;
    protected readonly IViewDomain ViewDomain;
    protected readonly Dictionary<Type, object> PageConstructorParamValues = new Dictionary<Type, object>();
    private readonly TableDataImageCommand _internalTableDataCommand;
    private readonly ImageCommand _internalCommand;
    public Func<IList, bool> CanExecuteFunc { get; set; }
        
    public enum TableDataReferenceMode
    {
        Auto,
        Yes,
        No
    }

    public TableDataReferenceMode UseTableDataReference { get; set; } = TableDataReferenceMode.Auto;

    private TableDataReferenceMode GetTableDataReferenceMode()
    {
        if (UseTableDataReference == TableDataReferenceMode.Auto)
        {
            if (Filter != null)
                return TableDataReferenceMode.Yes;

            return TableDataReferenceMode.No;
        }

        return UseTableDataReference;
    }

    public OpenPageCommand(Type pageType, IViewDomain viewDomain = null, IDataDomainScope dataDomainScope = null)
    {
        PageType = pageType;
        ViewDomain = viewDomain ?? Kapok.View.ViewDomain.Default;
        PageConstructorParamValues.Add(typeof(IDataDomainScope), dataDomainScope);
        PageConstructorParamValues.Add(typeof(IViewDomain), viewDomain);
        _internalTableDataCommand = new TableDataImageCommand(Execute, CanExecute);
        _internalCommand = new ImageCommand(Execute, CanExecute);
    }
    public OpenPageCommand(Type pageType, Func<bool> canExecuteFunc, IViewDomain viewDomain = null, IDataDomainScope dataDomainScope = null)
    {
        PageType = pageType;
        ViewDomain = viewDomain ?? Kapok.View.ViewDomain.Default;
        PageConstructorParamValues.Add(typeof(IDataDomainScope), dataDomainScope);
        PageConstructorParamValues.Add(typeof(IViewDomain), viewDomain);

        if (canExecuteFunc != null)
            CanExecuteFunc = (o) => canExecuteFunc.Invoke();

        _internalTableDataCommand = new TableDataImageCommand(Execute, CanExecute);
        _internalCommand = new ImageCommand(Execute, CanExecute);
    }
    public OpenPageCommand(Type pageType, Func<IList, bool> canExecuteFunc, IViewDomain viewDomain = null, IDataDomainScope dataDomainScope = null)
    {
        PageType = pageType;
        ViewDomain = viewDomain ?? Kapok.View.ViewDomain.Default;
        PageConstructorParamValues.Add(typeof(IDataDomainScope), dataDomainScope);
        PageConstructorParamValues.Add(typeof(IViewDomain), viewDomain);
        CanExecuteFunc = canExecuteFunc;
        _internalTableDataCommand = new TableDataImageCommand(Execute, CanExecute);
        _internalCommand = new ImageCommand(Execute, CanExecute);
    }

    private Type GetTSourceEntryType()
    {
        foreach (var @interface in PageType.GetInterfaces())
        {
            if (@interface.IsGenericType &&
                @interface.GetGenericTypeDefinition() == typeof(IDataPage<>))
            {
                return @interface.GenericTypeArguments[0];
            }
        }

        throw new NotImplementedException("The TPage type does not implement IDataPage<TEntry>.");
    }

    private IFilterSet ConstructFilterSet()
    {
        var filterSetType = typeof(FilterSet<>).MakeGenericType(GetTSourceEntryType());

        var emptyConstructorInfo = filterSetType.GetConstructor(new Type[] { });
        if (emptyConstructorInfo == null)
            throw new NotSupportedException("Internal error: FilterSet<> does not have an empty constructor.");

        return (IFilterSet)emptyConstructorInfo.Invoke(null);
    }

    protected virtual void Execute()
    {
        var page = ViewDomain.ConstructPage(PageType, PageConstructorParamValues);
        page.Show();
    }

    protected virtual void Execute(IList selectedEntries)
    {
        if (selectedEntries == null || selectedEntries.Count == 0)
        {
            Debug.WriteLine("Canceled OpenPageCommand: No entry selected.");
            return;
        }

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
            Debug.WriteLine("Canceled OpenPageCommand: only {NewItemPlaceholder} entries where selected.");
            return;
        }

        var page = (IDataPage)ViewDomain.ConstructPage(PageType, PageConstructorParamValues);

        if (Filter != null)
        {
            var filterSet = ConstructFilterSet();

            var firstEntry = newSelectedEntriesList[0];
            Filter.Invoke(filterSet, firstEntry, BaseDataSetView?.Filter.GetNestedDataFilter(ViewDomain) ?? new Dictionary<string, object>());

            page.DataSet.Filter.Add(filterSet);
        }

        page.Show();
    }

    protected virtual bool CanExecute()
    {
        return CanExecuteFunc?.Invoke(null) ?? true;
    }

    protected virtual bool CanExecute(IList selectedEntries)
    {
        if (selectedEntries == null)
            return false;

        if (selectedEntries.Count != 1)
            return false;

        if (selectedEntries[0] != null)
            return CanExecuteFunc?.Invoke(selectedEntries) ?? true;

        return false;
    }

    public IDataSetView BaseDataSetView { get; set; }

    public Action<IFilterSet, object, IReadOnlyDictionary<string, object>> Filter { get; set; }

    #region ICommand

    bool ICommand.CanExecute(object parameter)
    {
        switch (GetTableDataReferenceMode())
        {
            case TableDataReferenceMode.Yes:
                return _internalTableDataCommand.CanExecute(parameter);
            case TableDataReferenceMode.No:
                return _internalCommand.CanExecute(parameter);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void ICommand.Execute(object parameter)
    {
        switch (GetTableDataReferenceMode())
        {
            case TableDataReferenceMode.Yes:
                _internalTableDataCommand.Execute(parameter);
                return;
            case TableDataReferenceMode.No:
                _internalCommand.Execute(parameter);
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    event EventHandler ICommand.CanExecuteChanged
    {
        add
        {
            _internalCommand.CanExecuteChanged += value;
            _internalTableDataCommand.CanExecuteChanged += value;
        }
        remove
        {
            _internalCommand.CanExecuteChanged -= value;
            _internalTableDataCommand.CanExecuteChanged -= value;
        }
    }

    #endregion

    #region IImageCommand

    public string ImageName
    {
        get => _internalCommand.ImageName;
        set => _internalCommand.ImageName = value;
    }

    string IImageCommand.SmallImage => _internalCommand.SmallImage;

    string IImageCommand.LargeImage => _internalCommand.LargeImage;

    #endregion
}

public class OpenPageCommand<TPage, TSourceEntry, TDestinationEntry> : OpenPageCommand
    where TPage : IDataPage<TDestinationEntry>
    where TSourceEntry : class, new()
    where TDestinationEntry : class, new()
{
    public OpenPageCommand(IViewDomain viewDomain = null, IDataDomainScope dataDomainScope = null)
        : base(typeof(TPage), viewDomain, dataDomainScope)
    {
    }

    public OpenPageCommand(Func<bool> canExecuteFunc, IViewDomain viewDomain = null, IDataDomainScope dataDomainScope = null)
        : base(typeof(TPage), canExecuteFunc, viewDomain, dataDomainScope)
    {
    }

    public OpenPageCommand(Func<IList, bool> canExecuteFunc, IViewDomain viewDomain = null, IDataDomainScope dataDomainScope = null)
        : base(typeof(TPage), canExecuteFunc, viewDomain, dataDomainScope)
    {
    }

    private Action<IFilterSet<TDestinationEntry>, TSourceEntry, IReadOnlyDictionary<string, object>> _filter;
    public new Action<IFilterSet<TDestinationEntry>, TSourceEntry, IReadOnlyDictionary<string, object>> Filter
    {
        get => _filter;
        set
        {
            if (_filter == value) return;
            _filter = value;
            ((OpenPageCommand) this).Filter = (filter, entry, filterData) =>
                value.Invoke((IFilterSet<TDestinationEntry>) filter, (TSourceEntry) entry, filterData);
        }
    }
}