using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq.Expressions;
using System.Windows.Data;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.View.Wpf;

public interface IWpfDataSetView : IDataSetView
{
    Color? GetForegroundColorOfEntity(object entity, string? propertyName = null);
    Color? GetBackgroundColorOfEntity(object entity, string? propertyName = null);
    Color? GetForegroundSelectedColorOfEntity(object entity, string? propertyName = null);
    Color? GetBackgroundSelectedColorOfEntity(object entity, string? propertyName = null);
}

public class WpfDataSetView<TEntry> : DataSetView<TEntry>, IWpfDataSetView
    where TEntry : class, new()
{
    private const string NewItemPlaceholderTypeString = "{NewItemPlaceholder}";

    protected readonly CollectionViewSource CollectionViewSource = new();

    public WpfDataSetView(IServiceProvider serviceProvider, IDataDomainScope dataDomainScope, IEntityService<TEntry>? entityService = null)
        : base(serviceProvider, dataDomainScope, entityService)
    {
        FilterView = new FilterSetView<TEntry>(Filter);
        FilterView.ApplyFilter += FilterView_ApplyFilter;

        // init commands
        ToggleEditFilterAction = new UIToggleAction("EditFilter", ToggleEditFilter) {Image = "filter-edit"};
    }

    private void FilterView_ApplyFilter(object? sender, EventArgs e)
    {
        if (ToggleEditFilterAction.IsChecked)
            ToggleEditFilterAction.IsChecked = false;

        if (CanSave())
            // TODO: we don't throw an message here if we shall save changes!!; they will be lost after calling Refresh()!
            throw new NotImplementedException();

        Refresh();
    }

    public override void Dispose()
    {
        base.Dispose();

        FilterView.ApplyFilter -= FilterView_ApplyFilter;

        // remove references to avoid memory leaks
        CollectionViewSource.Source = null;
    }

    public ICollectionView View
    {
        get
        {
            if (CollectionViewSource.Source == null)
                RefreshCollectionViewSource();

            return CollectionViewSource.View;
        }
    }

    public new object? Current
    {
        get => base.Current;
        set
        {
            if (value is TEntry entry)
                base.Current = entry;
            else
                // NOTE: Set to 'null' here if type does not match --> when the {NewItemPlaceholder} line is selected, not the 'TEntry' type is given. We set it to 'null' because we don't want to have any interaction with 'Current' in this point in time.
                base.Current = null;
        }
    }

    #region AsQueryable View

    public IQueryable<TEntry> GetViewAsQueryable()
    {
        if (View.CanGroup && View.GroupDescriptions.Count > 0)
            // TODO: this is not implemented
            throw new NotImplementedException($"The method {nameof(GetViewAsQueryable)} does not support views with grouping.");

        var queryable = Collection.AsQueryable();

        if (View.Filter != null)
        {
            queryable = queryable.Where(e => View.Filter.Invoke(e));
        }

        //tableData.View.SortDescriptions.Add(new SortDescription(nameof(ISortableEntity.SortOrder), ListSortDirection.Ascending));

        bool firstSort = true;

        IOrderedQueryable<TEntry> orderedQueryable = null;

        foreach (var viewSortDescription in View.SortDescriptions)
        {
            var propertyInfo = typeof(TEntry).GetProperty(viewSortDescription.PropertyName);

            if (propertyInfo == null)
            {
                Debug.WriteLine($"WARNING: The sort descriptor for property {viewSortDescription.PropertyName} could not be found in the entity {typeof(TEntry).FullName}.");
                continue;
            }

            if (firstSort)
            {
                Expression<Func<TEntry, object>> keySelector = e => propertyInfo.GetValue(e, null);
                orderedQueryable = viewSortDescription.Direction == ListSortDirection.Ascending
                    ? queryable.OrderBy(keySelector)
                    : queryable.OrderByDescending(keySelector);
                firstSort = false;
            }
            else
            {
                Expression<Func<TEntry, object>> keySelector = e => propertyInfo.GetValue(e, null);
                orderedQueryable = viewSortDescription.Direction == ListSortDirection.Ascending
                    ? orderedQueryable.ThenBy(keySelector)
                    : orderedQueryable.ThenByDescending(keySelector);
            }
        }

        return orderedQueryable ?? queryable;
    }

    public TEntry GetNextEntry()
    {
        if (Current == null)
            return null;

        var virtualView = GetViewAsQueryable();

        return virtualView?.GetNextOrDefault((TEntry)Current);
    }

    public TEntry GetPreviousEntry()
    {
        if (Current == null)
            return null;

        var virtualView = GetViewAsQueryable();

        return virtualView?.GetPreviousOrDefault((TEntry)Current);
    }

    #endregion

    #region Filter
        
    protected override void OnFilterChanged()
    {
        FilterView?.Reset();

        base.OnFilterChanged();
    }

    public IToggleAction ToggleEditFilterAction { get; }

    private void ToggleEditFilter()
    {
    }

    #endregion

    protected void RefreshCollectionViewSource()
    {
        CollectionViewSource.Source = null;
        CollectionViewSource.Source = Collection;
    }

    public override void Load()
    {
        // This is necessary because the filter could have been changed by the DataGrid filter which is not connected to the FilterView object
        FilterView.Reset();

        base.Load();
    }

    public override void Refresh()
    {
        base.Refresh();

        RefreshCollectionViewSource();
    }

    #region Commands

    protected override bool CanCreateNewEntry()
    {
        return View is IEditableCollectionView && base.CanCreateNewEntry();
    }

    protected override void CreateNewEntry()
    {
        if (View is IEditableCollectionView editableView)
        {
            if (editableView.IsAddingNew)
                // NOTE: Here the code will crash with an exception when sorting or grouping is used in the View.
                //       Seems to be an .NET bug, see here as well: https://www.codesd.com/item/an-exception-is-thrown-when-adding-an-item-to-listcollectionview.html
                editableView.CommitNew();
            else if (editableView.IsEditingItem)
                editableView.CommitEdit();
        }
            
        base.CreateNewEntry();
    }

    protected override bool CanDeleteEntry(IList<TEntry> selectedEntries)
    {
        if (View is IEditableCollectionView editableView)
        {
            if (editableView.IsAddingNew)
                // We disable here the remove because this would lead to that the row
                // 'NewItemPlaceholder' is not anymore shown.
                // TODO: It should be checked if that does not lead to a session lock when for example an entry can not be validated and a removal is not possible!
                return false;
        }

        if (selectedEntries != null &&
            selectedEntries.Count == 1 && // we don't show the delete button only when we just selected the NewItemPlaceholder; otherwise, the NewItemPlaceholder will be filtered out in the execution method
            selectedEntries.Cast<object>().Any(e => e.GetType() != typeof(TEntry) &&
                                                    e.ToString() == NewItemPlaceholderTypeString))
        {
            // a deletion can not be executed when one selected line is a NewItemPlaceholder (not a real line)
            return false;
        }

        return base.CanDeleteEntry(selectedEntries);
    }

    protected override void DeleteEntry(IList<TEntry> selectedEntries)
    {
        if (selectedEntries == null) return;

        if (CollectionViewSource.View is IEditableCollectionView editableCollectionView)
        {
            if (editableCollectionView.IsAddingNew)
                editableCollectionView.CancelNew();

            if (editableCollectionView.IsEditingItem)
            {
                editableCollectionView.CancelEdit();

                if (IsNewEntry)
                {
                    // delete the editing entry from the deletion list
                    Debug.Assert(Current != null);
                    Debug.Assert(selectedEntries.Contains(Current));
                    selectedEntries.Remove((TEntry)Current);

                    Debug.Assert(Collection.Contains(Current));
                    Collection.Remove((TEntry)Current);
                }
            }
        }

        if (selectedEntries.Count > 0)
            base.DeleteEntry(selectedEntries);
    }

    protected override bool CanToggleFilterVisible()
    {
        return true;
    }

    protected override void ToggleFilterVisible()
    {
        IsFilterVisible = !IsFilterVisible;
    }

    #endregion

    #region Color styling

    // NOTE: a cache for the coloring would be interesting here

    public Color? GetForegroundColorOfEntity(TEntry entity, string propertyName = null)
    {
        var e = new DataSetEntityColoringEventArgs(entity, propertyName);
        RaiseEntryColoring(e);
        return e.ForegroundColor;
    }

    public Color? GetBackgroundColorOfEntity(TEntry entity, string propertyName = null)
    {
        var e = new DataSetEntityColoringEventArgs(entity, propertyName);
        RaiseEntryColoring(e);
        return e.BackgroundColor;
    }

    public Color? GetForegroundSelectedColorOfEntity(TEntry entity, string propertyName = null)
    {
        var e = new DataSetEntityColoringEventArgs(entity, propertyName);
        RaiseEntryColoring(e);
        return e.ForegroundSelectedColor;
    }

    public Color? GetBackgroundSelectedColorOfEntity(TEntry entity, string propertyName = null)
    {
        var e = new DataSetEntityColoringEventArgs(entity, propertyName);
        RaiseEntryColoring(e);
        return e.BackgroundSelectedColor;
    }

    #endregion

    #region IWpfDataSetView

    Color? IWpfDataSetView.GetForegroundColorOfEntity(object entity, string propertyName)
    {
        if (entity == null)
            return null;

        var typedEntity = entity as TEntry;
        if (typedEntity == null)
        {
            return null;
        }

        return GetForegroundColorOfEntity((TEntry)entity, propertyName);
    }

    Color? IWpfDataSetView.GetBackgroundColorOfEntity(object entity, string propertyName)
    {
        if (entity == null)
            return null;

        var typedEntity = entity as TEntry;
        if (typedEntity == null)
        {
            return null;
        }

        return GetBackgroundColorOfEntity((TEntry)entity, propertyName);
    }

    Color? IWpfDataSetView.GetForegroundSelectedColorOfEntity(object entity, string propertyName)
    {
        if (entity == null)
            return null;

        var typedEntity = entity as TEntry;
        if (typedEntity == null)
        {
            return null;
        }

        return GetForegroundSelectedColorOfEntity((TEntry)entity, propertyName);
    }

    Color? IWpfDataSetView.GetBackgroundSelectedColorOfEntity(object entity, string propertyName)
    {
        if (entity == null)
            return null;

        var typedEntity = entity as TEntry;
        if (typedEntity == null)
        {
            return null;
        }

        return GetBackgroundSelectedColorOfEntity((TEntry)entity, propertyName);
    }

    #endregion
}