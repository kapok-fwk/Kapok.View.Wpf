using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Entity;

namespace Kapok.View.Wpf;

public class FilterSetView<TEntry> : IFilterSetView
    where TEntry : class
{
    private readonly List<PropertyViewModel> _filterableProperties = new();
    private bool _collectionHasChanged;

    public FilterSetView(IFilterSet<TEntry> filterSet)
    {
        if (!(filterSet is FilterBase<TEntry>))
            throw new ArgumentException("The parameter filterSet must have FilterBase<TEntry> as base class.", nameof(filterSet));

        FilterSet = filterSet;

        // NOTE: we could optimize this call so that it is only called when required (=> FilterablePropertiesView.View is called from the WPF/UI)
        LoadFilterableProperties();

        FilterablePropertiesView = new QueryableCollectionViewSource<PropertyViewModel>
        {
            QueryableSource = _filterableProperties.AsQueryable()
        };

        DeleteFilterablePropertyAction = new UIAction<object>("DeleteFilterableProperty", DeleteFilterableProperty, CanDeleteFilterableProperty) {Image = "symbol-delete"};

        ClearAction = new UIAction("FilterSet.Clear", Clear, CanClear);
        ResetAction = new UIAction("FilterSet.Reset", Reset, CanReset);
        ApplyAction = new UIAction("FilterSet.Apply", Apply, CanApply) {Image = "execute"};

        PropertyFilters = new ObservableCollection<PropertyFilterViewModel<TEntry>>();
        PropertyFilters.CollectionChanged += PropertyFilters_CollectionChanged;
    }

    private void PropertyFilters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Replace ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            _collectionHasChanged = true;
        }
    }

    private void ResetCollectionHasChanged()
    {
        _collectionHasChanged = false;

        foreach (var propertyFilter in PropertyFilters)
            propertyFilter.ValueIsChanged = false;
    }

    private void LoadFilterableProperties()
    {
        var propertyList = typeof(TEntry).GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
            .ToList();

        // remove data partition properties
        foreach (var dataPartition in DataDomain.Default?.DataPartitions.Values ?? new List<DataPartition>())
        {
            if (!dataPartition.InterfaceType.IsAssignableFrom(typeof(TEntry)))
                continue;

            propertyList.Remove(
                propertyList.FirstOrDefault(p => p.Name == dataPartition.PartitionProperty.Name)
            );
        }

        if (typeof(INotifyDataErrorInfo).IsAssignableFrom(typeof(TEntry)))
        {
            propertyList.Remove(
                propertyList.FirstOrDefault(p => p.Name == nameof(INotifyDataErrorInfo.HasErrors))
            );
        }

        foreach (var propertyInfo in propertyList)
        {
            var autoCalculateAttribute = propertyInfo.GetCustomAttribute<AutoCalculateAttribute>();
            if (autoCalculateAttribute != null)
            {
                // TODO as of today, we don't filtering on auto-calculate attributes, but that would be an great feature
                continue;
            }

            if (propertyInfo.PropertyType.IsClass &&
                propertyInfo.PropertyType != typeof(string))
            {
                // skip all complex, non-value types
                continue;
            }

            if (propertyInfo.PropertyType.IsGenericType)
            {
                var genericType = propertyInfo.PropertyType.GetGenericTypeDefinition();
                if (genericType == typeof(IEnumerable<>) ||
                    genericType == typeof(IList<>) ||
                    genericType == typeof(ICollection<>))
                {
                    // TODO: we should implement here the possibility to filter on 'NestedProperty.Count()' e.g. filter on all journal transactions which have more than 2 entries

                    // skip nested data fields
                    continue;
                }
            }

            _filterableProperties.Add(new PropertyViewModel(propertyInfo));
        }
    }

    public IFilterSet<TEntry> FilterSet { get; }

    public IReadOnlyList<PropertyViewModel> FilterableProperties => _filterableProperties;

    public QueryableCollectionViewSource<PropertyViewModel> FilterablePropertiesView { get; }

    public ObservableCollection<PropertyFilterViewModel<TEntry>> PropertyFilters { get; }

    private void IterateThroughFilterSet(Action<FilterLayer, IFilter> iteration)
    {
        foreach (var layerPair in FilterSet.Layers)
        {
            if (layerPair.Value is IPropertyFilterCollection<TEntry> propertyFilterCollection)
            {
                foreach (var propertyFilter in propertyFilterCollection.Properties)
                {
                    iteration.Invoke(layerPair.Key, propertyFilter);
                }
            }
            else
            {
                // TODO: show something that there is an hidden filter ...
                iteration.Invoke(layerPair.Key, layerPair.Value);
            }
        }
    }

    private void LoadPropertyFiltersFromFilterSet()
    {
        PropertyFilters.Clear();

        IterateThroughFilterSet((layer, filter) =>
        {
            if (!(filter is IPropertyFilter propertyFilter))
                return;

            var filterViewModel = new PropertyFilterViewModel<TEntry>();
            filterViewModel.FilterLayer = layer;
            filterViewModel.PropertyFilter = propertyFilter;

            // only the user layer filter is editable via the view
            filterViewModel.IsReadOnly = layer != FilterLayer.User;

            var propertyInfo = propertyFilter.PropertyInfo;
            filterViewModel.Property =
                FilterableProperties.FirstOrDefault(p => p.PropertyInfo.Name == propertyInfo.Name);

            if (filterViewModel.Property == null)
                return; // the filter is filtered in the background, but since it is not added to the 'FilterableProperties', it shall not be shown in the view

            var filterString = propertyFilter.AsFilterString();

            if (filterString == null)
            {
                filterViewModel.IsReadOnly = true;
                filterViewModel.Value = "(internal)"; // TODO: translation missing
            }
            else
            {
                filterViewModel.Value = filterString;
            }

            PropertyFilters.Add(filterViewModel);
        });

        ResetCollectionHasChanged();
    }

    public IAction<PropertyFilterViewModel<TEntry>> DeleteFilterablePropertyAction { get; }

    private void DeleteFilterableProperty(object selectedEntry)
    {
        if (selectedEntry is PropertyFilterViewModel<TEntry> propertyFilterViewModel)
            if (PropertyFilters.Contains(propertyFilterViewModel))
                PropertyFilters.Remove(propertyFilterViewModel);
    }

    private bool CanDeleteFilterableProperty(object selectedEntry)
    {
        return selectedEntry is PropertyFilterViewModel<TEntry>;
    }

    /// <summary>
    /// Clear all user filters
    /// </summary>
    public void Clear()
    {
        foreach (var propertyFilter in (from p in PropertyFilters
                     where p.FilterLayer == FilterLayer.User
                     select p).ToList())
        {
            PropertyFilters.Remove(propertyFilter);
        }
    }

    private bool CanClear()
    {
        return PropertyFilters.Count != 0;
    }

    /// <summary>
    /// Reset the view model filters to the filters in the filter set.
    /// </summary>
    public void Reset()
    {
        LoadPropertyFiltersFromFilterSet();
    }

    private bool CanReset()
    {
        return _collectionHasChanged && PropertyFilters.Any(vm => !vm.ValueIsChanged);
    }
        
    /// <summary>
    /// Apply the filter changes in the view model to the filter set.
    /// </summary>
    public void Apply()
    {
        using (((FilterBase<TEntry>)FilterSet).DeferFilterChange())
        {
            // remove filters which are removed in UI
            foreach (var filter in FilterSet.UserLayer.Properties
                         .Except(from p in PropertyFilters
                             where p.PropertyFilter != null
                             select p.PropertyFilter)
                         .ToList())
            {
                FilterSet.UserLayer.Properties.Remove(filter);
            }

            // TODO: need something in Filter.UserLayer to pause FilterChanged updates during this loop

            foreach (var filterViewModel in (from f in PropertyFilters
                         where f.IsReadOnly == false
                         select f).ToList())
            {
                if (filterViewModel.FilterLayer != FilterLayer.User)
                    throw new NotSupportedException("Only user layer filter should be editable via the view.");

                if (filterViewModel.PropertyFilter != null &&
                    !FilterSet.UserLayer.Properties.Contains(filterViewModel.PropertyFilter))
                    throw new NotSupportedException("The filter has already been deleted and can not be filtered.");

                void DeleteFilter()
                {
                    FilterSet.UserLayer.Properties.Remove(filterViewModel.PropertyFilter);

                    filterViewModel.PropertyFilter = null;
                }

                void AddNewFilter()
                {
                    var filter = new PropertyFilterStringFilter<TEntry>(filterViewModel.Property.PropertyInfo);
                    filter.FilterString = filterViewModel.Value.ToString();
                    FilterSet.UserLayer.Properties.Add(filter);

                    filterViewModel.PropertyFilter = filter;
                }

                if (filterViewModel.PropertyFilter is IPropertyStaticFilter<TEntry>)
                {
                    DeleteFilter();
                    AddNewFilter();
                }
                else if (filterViewModel.PropertyFilter is IPropertyFilterStringFilter<TEntry> stringFilter)
                {
                    if (filterViewModel.Value == null || string.IsNullOrWhiteSpace(filterViewModel.Value.ToString()))
                    {
                        DeleteFilter();
                        PropertyFilters.Remove(filterViewModel);
                    }
                    else if (stringFilter.FilterString != filterViewModel.Value.ToString())
                    {
                        stringFilter.FilterString = filterViewModel.Value.ToString();
                    }
                }
                else if (filterViewModel.PropertyFilter == null)
                {
                    AddNewFilter();
                }
                else
                {
                    throw new NotSupportedException("Unexpected filter to be changed.");
                }
            }

            OnApplyFilter();
            ResetCollectionHasChanged();
        }
    }

    private bool CanApply()
    {
        return _collectionHasChanged || PropertyFilters.Any(vm => vm.ValueIsChanged);
    }

    public IAction ClearAction { get; }

    public IAction ResetAction { get; }

    public IAction ApplyAction { get; }


    public event EventHandler ApplyFilter;

    protected void OnApplyFilter()
    {
        ApplyFilter?.Invoke(this, EventArgs.Empty);
    }
}