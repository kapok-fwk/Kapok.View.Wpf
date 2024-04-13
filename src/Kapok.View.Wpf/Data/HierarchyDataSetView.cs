using System.Collections.ObjectModel;
#if DEBUG
using System.Diagnostics;
#endif
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Entity;

namespace Kapok.View.Wpf;

public class HierarchyDataSetView<TEntry> : WpfDataSetView<TEntry>, IHierarchyDataSetView<TEntry>
    where TEntry : class, IHierarchyEntry<TEntry>, new()
{
    // TODO: After 'Refresh()' the information which row is opened and closed is removed. This should be changed, maybe by using an proxy entry and a cache in the HierarchyTableDataViewModel object which is mapped back to the objects which still exist.

    public HierarchyDataSetView(IServiceProvider serviceProvider, IDataDomainScope dataDomainScope, IEntityService<TEntry>? entityService = null)
        : base(serviceProvider, dataDomainScope, entityService)
    {
        AllEntriesCollection = new ObservableCollection<TEntry>();
        AllEntriesCollection.CollectionChanged += AllEntriesCollection_CollectionChanged;
        Collection.CollectionChanged += Collection_CollectionChanged;

        ExpandAction = new UIAction("Expand", () => Expand((TEntry)Current), CanExpand);
        CollapseAction = new UIAction("Collapse", () => Collapse((TEntry)Current), CanCollapse);
        ToggleAction = new UIAction("Toggle", () => Toggle((TEntry)Current), CanToggle);
        MoveInAction = new UIAction("MoveIn", MoveIn, CanMoveIn);
        MoveOutAction = new UIAction("MoveOut", MoveOut, CanMoveOut);
    }

    public override void Dispose()
    {
        base.Dispose();
        AllEntriesCollection.CollectionChanged -= AllEntriesCollection_CollectionChanged;
        Collection.CollectionChanged -= Collection_CollectionChanged;
    }

    private bool _firstLoad = true;

    private void SetNewEntryDefaults(TEntry entry)
    {
        if (entry.Level == 0)
            entry.IsVisible = true;

        entry.HasChildren = AllEntriesCollection.Any(entry.GetChildrenPredicate());

        if (entry.IsExpanded)
            entry.IsExpanded = false;

        if (entry.Parent != null && !entry.Parent.HasChildren)
        {
            entry.Parent.HasChildren = true;
        }
    }

    protected override void OnLoad(List<TEntry> newEntries)
    {
        List<TEntry> entriesToHide = new List<TEntry>();

        foreach (var newEntry in newEntries)
        {
            if (_firstLoad)
            {
                // TODO: move these three fields into the WPF view; they shouldn't be in here, nor be in the property
                newEntry.IsExpanded = default;
                newEntry.IsVisible = default;
                newEntry.HasChildren = default;
            }

            SetNewEntryDefaults(newEntry);

            if (!newEntry.IsVisible)
                entriesToHide.Add(newEntry);
        }

        lock (_syncCollectionLockObject)
        {
            _syncCollections = true;
            AllEntriesCollection.AddRange(newEntries);
            _syncCollections = false;
        }

        newEntries.RemoveRange(entriesToHide);

        _firstLoad = false;

        base.OnLoad(newEntries);
    }

    #region Collection synchronisation

    private void AllEntriesCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncCollections)
            return;

        bool addItems = false;
        bool oldItems = false;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                addItems = true;
                break;
            case NotifyCollectionChangedAction.Replace:
                addItems = true;
                oldItems = true;
                break;
            case NotifyCollectionChangedAction.Remove:
                oldItems = true;
                break;
            case NotifyCollectionChangedAction.Move:
                lock (_syncCollectionLockObject)
                {
                    _syncCollections = true;
                    Collection.Move(e.OldStartingIndex, e.NewStartingIndex);
                    _syncCollections = false;
                }
                return;
            case NotifyCollectionChangedAction.Reset:
                lock (_syncCollectionLockObject)
                {
                    _syncCollections = true;
                    Collection.Clear();
                    _syncCollections = false;
                }
                break;
            default:
                throw new NotSupportedException();
        }

        if (addItems && e.NewItems != null)
        {
            var newVisibleItems = new List<TEntry>();

            foreach (var newItem in e.NewItems.Cast<TEntry>())
            {
                SetNewEntryDefaults(newItem);

                if (newItem.IsVisible)
                    newVisibleItems.Add(newItem);
            }

            if (newVisibleItems.Count == 1)
            {
                lock (_syncCollectionLockObject)
                {
                    _syncCollections = true;
                    Collection.Add(newVisibleItems[0]);
                    _syncCollections = false;
                }
            }
            else
            {
                lock (_syncCollectionLockObject)
                {
                    _syncCollections = true;
                    Collection.AddRange(newVisibleItems);
                    _syncCollections = false;
                }
            }
        }

        if (oldItems && e.OldItems != null)
        {
            var oldItemsToRemove = new List<TEntry>();

            foreach (var oldItem in e.OldItems.Cast<TEntry>())
            {
                if (Collection.Contains(oldItem))
                    oldItemsToRemove.Add(oldItem);
            }

            if (oldItemsToRemove.Count == 1)
            {
                lock (_syncCollectionLockObject)
                {
                    _syncCollections = true;
                    Collection.Remove(oldItemsToRemove[0]);
                    _syncCollections = false;
                }
            }
            else if (oldItemsToRemove.Count > 1)
            {
                lock (_syncCollectionLockObject)
                {
                    _syncCollections = true;
                    Collection.RemoveRange(oldItemsToRemove);
                    _syncCollections = false;
                }
            }
        }
    }

    private void Collection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncCollections)
            return;

        bool addItems = false;
        bool oldItems = false;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                addItems = true;
                break;
            case NotifyCollectionChangedAction.Replace:
                addItems = true;
                oldItems = true;
                break;
            case NotifyCollectionChangedAction.Remove:
                oldItems = true;
                break;
            case NotifyCollectionChangedAction.Move:
                lock (_syncCollectionLockObject)
                {
                    _syncCollections = true;
                    AllEntriesCollection.Move(e.OldStartingIndex, e.NewStartingIndex);
                    _syncCollections = false;
                }
                return;
            case NotifyCollectionChangedAction.Reset:
                lock (_syncCollectionLockObject)
                {
                    _syncCollections = true;
                    AllEntriesCollection.Clear();
                    _syncCollections = false;
                }
                break;
            default:
                throw new NotSupportedException();
        }

        if (addItems && e.NewItems != null)
        {
            if (e.NewStartingIndex != -1)
            {
                if (e.NewItems.Count == 1)
                {
                    _syncCollections = true;
                    AllEntriesCollection.Insert(e.NewStartingIndex, e.NewItems.Cast<TEntry>().First());
                    _syncCollections = false;
                }
                else if (e.NewItems.Count > 1)
                {
                    _syncCollections = true;
                    AllEntriesCollection.InsertRange(e.NewStartingIndex, e.NewItems.Cast<TEntry>());
                    _syncCollections = false;
                }
            }
            else
            {
                if (e.NewItems.Count == 1)
                {
                    _syncCollections = true;
                    AllEntriesCollection.Add(e.NewItems.Cast<TEntry>().First());
                    _syncCollections = false;
                }
                else if (e.NewItems.Count > 1)
                {
                    _syncCollections = true;
                    AllEntriesCollection.AddRange(e.NewItems.Cast<TEntry>());
                    _syncCollections = false;
                }
            }
        }

        if (oldItems && e.OldItems != null)
        {
            // get a list of all parent items which need to be checked if they don't have a child item anymore after the deletion is executed
            var parentsToCheck = (
                from item in e.OldItems.Cast<TEntry>()
                group item by item.Parent
                into g
                where g.Key != null &&
                      !e.OldItems.Cast<TEntry>().Contains(g.Key) &&
                      g.Key.HasChildren
                select g.Key
            ).ToList();

            // execute the deletion
            if (e.OldItems.Count == 1)
            {
                _syncCollections = true;
                AllEntriesCollection.Remove(e.OldItems.Cast<TEntry>().First());
                _syncCollections = false;
            }
            else if (e.OldItems.Count > 1)
            {
                _syncCollections = true;
                AllEntriesCollection.RemoveRange(e.OldItems.Cast<TEntry>());
                _syncCollections = false;
            }

            // updates the HasChildren property of the parents
            foreach (var parentItem in parentsToCheck)
            {
                parentItem.HasChildren = AllEntriesCollection.Any(parentItem.GetChildrenPredicate());
            }
        }
    }

    private readonly object _syncCollectionLockObject = new object();
    private bool _syncCollections;

    /// <summary>
    /// A collection containing all entries, including the not visible once.
    /// </summary>
    private ObservableCollection<TEntry> AllEntriesCollection { get; }

    #endregion

    #region Tree management commands

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    public IAction ExpandAction { get; }
    public IAction CollapseAction { get; }

    /// <summary>
    /// This command is a combination of ExpandCommand and CollapseCommand:
    /// When it is collapsed it will expand, when it is expanded it will collapse.
    /// </summary>
    public IAction ToggleAction { get; }

    public IAction MoveInAction { get; }
    public IAction MoveOutAction { get; }
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global

    protected virtual void Expand(TEntry entry)
    {
        if (View is IEditableCollectionView editableCollection)
        {
            if (editableCollection.IsAddingNew)
            {
                editableCollection.CommitNew();
            }

            if (editableCollection.IsEditingItem)
            {
                editableCollection.CommitEdit();
            }
        }

        if (!entry.IsExpanded)
            entry.IsExpanded = true;

        List<TEntry> childEntries = new List<TEntry>();
        foreach (var child in AllEntriesCollection.Where(entry.GetChildrenPredicate()))
        {
            if (!child.IsVisible)
            {
                child.IsVisible = true;
                childEntries.Add(child);
            }
        }

        // synchronize the change to the visible Collection
        lock (_syncCollectionLockObject)
        {
            _syncCollections = true;
            Collection.InsertRange(
                Collection.IndexOf(entry) + 1,
                childEntries
            );
            _syncCollections = false;
        }
    }

    private bool? _canExpandCache;
    protected virtual bool CanExpand()
    {
        if (Current == null) return false;

        if (_canExpandCache.HasValue)
            return _canExpandCache.Value;

        _canExpandCache = !((TEntry)Current).IsExpanded && AllEntriesCollection.Any(((TEntry)Current).GetChildrenPredicate());

        return _canExpandCache.Value;
    }

    private void CollapseLoop(TEntry currentEntry, List<TEntry> entriesToCollapse)
    {
        if (currentEntry.IsExpanded)
            currentEntry.IsExpanded = false;

        foreach (var child in AllEntriesCollection.Where(currentEntry.GetChildrenPredicate()))
        {
            child.IsVisible = false;
            entriesToCollapse.Add(child);

            if (child.IsExpanded)
            {
                CollapseLoop(child, entriesToCollapse);
            }
        }
    }

    protected virtual void Collapse(TEntry entry)
    {
        if (View is IEditableCollectionView editableCollection)
        {
            if (editableCollection.IsAddingNew)
            {
                editableCollection.CommitNew();
            }

            if (editableCollection.IsEditingItem)
            {
                editableCollection.CommitEdit();
            }
        }

        var entriesToCollapse = new List<TEntry>();

        CollapseLoop(entry, entriesToCollapse);

        // synchronize the change to the visible Collection
        lock (_syncCollectionLockObject)
        {
            _syncCollections = true;
            Collection.RemoveRange(entriesToCollapse);
            _syncCollections = false;
        }
    }

    private bool? _canCollapseCache;
    protected virtual bool CanCollapse()
    {
        if (Current == null) return false;

        if (_canCollapseCache.HasValue)
            return _canCollapseCache.Value;

        _canCollapseCache = ((TEntry)Current).IsExpanded && AllEntriesCollection.Any(((TEntry)Current).GetChildrenPredicate());

        return _canCollapseCache.Value;
    }

    protected virtual void Toggle(TEntry entry)
    {
        if (entry.IsExpanded)
            Collapse(entry);
        else
            Expand(entry);
    }

    private bool? _canToggleCache;
    protected virtual bool CanToggle()
    {
        if (Current == null) return false;

        if (_canToggleCache.HasValue)
            return _canToggleCache.Value;

        _canToggleCache = AllEntriesCollection.Any(((TEntry)Current).GetChildrenPredicate());

        return _canToggleCache.Value;
    }

    protected virtual void MoveIn()
    {
        if (View.CurrentItem == null) return;
        var current = (TEntry) View.CurrentItem;
        if (current.Level == 0) return;

        var currentParent = current.Parent;
        current.Parent = null;
        current.Level = current.Level - 1;

        currentParent.HasChildren = AllEntriesCollection.Any(currentParent.GetChildrenPredicate());
        if (currentParent.HasChildren == false)
        {
            currentParent.IsExpanded = false;
        }
        else
        {
            // TODO here we need something to resort it to the end or start of the parent children list  (if the item is sortable) ; or this will be covered at another code place
#if DEBUG
            Debug.WriteLine("HierarchyTableDataViewModel: MISS SORTING!!!");
#endif
        }
    }

    private bool? _canMoveInCache;
    protected virtual bool CanMoveIn()
    {
        if (_canMoveInCache.HasValue)
            return _canMoveInCache.Value;

        if (View.CurrentItem == null)
        {
            _canMoveInCache = false;
        }
        else
        {
            var current = (TEntry)View.CurrentItem;

            _canMoveInCache = current.Level > 0;
        }
            
        return _canMoveInCache.Value;
    }

    protected virtual void MoveOut()
    {
        if (View.CurrentItem == null) return;
        TEntry current = (TEntry) View.CurrentItem;

        if (!View.MoveCurrentToPrevious()) return;

        // search for parent
        int turnsToParent = 1;
        while (((TEntry)View.CurrentItem).Level != current.Level)
        {
            if (!View.MoveCurrentToPrevious())
            {
#if DEBUG
                Debug.WriteLine("HierarchyTableDataViewModel: MoveOut: Was not able to find parent of current item.");
#endif
                return;
            }

            turnsToParent++;
        }

        var parent = (TEntry) View.CurrentItem;
        if (parent.Parent == current.Parent)
        {
            // do the change
            current.Parent = parent;
            current.Level = parent.Level + 1; // we could do here just current.Level += 1;, but in case that the level is messed up this will fix the level for the item (at least in relation to its parent).

            if (!parent.HasChildren)
            {
                parent.HasChildren = true;
                parent.IsExpanded = true;
            }
            else if (!parent.IsExpanded)
            {
                Expand(parent);
            }
        }

        for (int i = 0; i < turnsToParent; i++)
        {
            View.MoveCurrentToNext();
        }
    }

    private bool? _canMoveOutCache;
    protected virtual bool CanMoveOut()
    {
        if (_canMoveOutCache.HasValue)
            // we use a cache here because the use of CanMoveOut over and over again in combination
            // with MoveCurrentToPrevious()/MoveCurrentToNext() leads to that the application consumes
            // tons of memory.
            // It looks like that MoveCurrentToPrevious()/MoveCurrentToNext() do something what the
            // .NET garbage collector is not able to clean up.
            return _canMoveOutCache.Value;

        if (View.CurrentItem == null) return false;
        TEntry current = (TEntry) View.CurrentItem;

        var previous = GetPreviousEntry();
        if (previous == null) return false;

        bool result = current.Level <= previous.Level;
#if DEBUG
        if (result)
        {
            if (previous.Level != current.Level)
                Debug.WriteLine("HierarchyTableDataViewModel: The hierarchy level is not correct in relation to the parent!");
            if (previous.IsVisible == false)
                Debug.WriteLine("HierarchyTableDataViewModel: The IsVisible field should be true in relation to the parent!");
        }
#endif

        _canMoveOutCache = result;

        return result;
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (propertyName == nameof(Current))
        {
            // clear cache
            _canExpandCache = null;
            _canCollapseCache = null;
            _canToggleCache = null;
            _canMoveInCache = null;
            _canMoveOutCache = null;
        }

        base.OnPropertyChanged(propertyName);
    }

    #endregion

    protected override void OnEntryPropertyChanged(TEntry? entry, string propertyName)
    {
        base.OnEntryPropertyChanged(entry, propertyName);

        if (propertyName != nameof(IHierarchyEntry<TEntry>.IsExpanded))
            return;

        if (entry == null)
            return;

        if (entry.HasChildren == false) // check via cached value if child entries exist
        {
            return;
        }

        if (entry.IsExpanded)
        {
            Expand(entry);
        }
        else
        {
            Collapse(entry);
        }
    }

    protected override bool CanToggleFilterVisible()
    {
        // As of today the filter is not supported in hierarchy views.
        return false;
    }
}