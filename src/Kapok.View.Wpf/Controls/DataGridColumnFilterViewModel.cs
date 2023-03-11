using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Kapok.BusinessLayer;

namespace Kapok.View.Wpf;

public class DataGridColumnFilterViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private const string FilterStringNotAvailable = "(*internal*)"; // TODO: translation missing
    private readonly CustomDataGrid _dataGrid;
    private readonly Type _elementType;
    private bool _isReadOnly;
    private string _queryString;

    public string PropertyBindingPath { get; }

    public DataGridColumnFilterViewModel(CustomDataGrid dataGrid, Type elementType, string propertyPath)
    {
        _dataGrid = dataGrid;
        _elementType = elementType;
        PropertyBindingPath = propertyPath;

        if (_dataGrid.Filter == null)
            throw new NotSupportedException("Filter not set to the DataGrid");

        SetQueryStringFromProperty();

        if (_dataGrid.Filter.Properties is INotifyCollectionChanged observableCollection)
        {
            // when a filter is already set programatically, execute here our registering/unregisering of events
            var propertyFilter = PropertyFilter;
            if (propertyFilter != null)
            {
                Filter_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new [] {propertyFilter}.ToList()));
            }

            CollectionChangedEventManager.AddHandler(observableCollection, Filter_CollectionChanged);
        }
    }

    private void Filter_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool addFilter;
        bool removeFilter;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                SetQueryStringInternal(string.Empty);
                return;
            case NotifyCollectionChangedAction.Add:
                addFilter = true;
                removeFilter = false;
                break;
            case NotifyCollectionChangedAction.Remove:
                addFilter = false;
                removeFilter = true;
                break;
            case NotifyCollectionChangedAction.Replace:
                addFilter = true;
                removeFilter = true;
                break;
            default:
                return;
        }

        if (addFilter)
        {
            var newFilter = e.NewItems?.Cast<IPropertyFilter>()
                .FirstOrDefault(pf => pf.PropertyInfo.Name == PropertyBindingPath);

            if (newFilter is ValidatableBindableObjectBase addItem &&
                addItem is IPropertyFilterStringFilter)
            {
                SetQueryStringFromProperty();
                addItem.PropertyChanged += PropertyFilter_PropertyChanged;
                addItem.ErrorsChanged += PropertyFilter_ErrorsChanged;
            }
        }

        if (removeFilter)
        {
            var oldFilter = e.OldItems?.Cast<IPropertyFilter>()
                .FirstOrDefault(pf => pf.PropertyInfo.Name == PropertyBindingPath);

            if (oldFilter is ValidatableBindableObjectBase removeItem &&
                removeItem is IPropertyFilterStringFilter)
            {
                SetQueryStringFromProperty();
                removeItem.PropertyChanged -= PropertyFilter_PropertyChanged;
                removeItem.ErrorsChanged -= PropertyFilter_ErrorsChanged;
            }
        }
    }

    private IPropertyFilter PropertyFilter => _dataGrid.Filter.Properties?.FirstOrDefault(d => d.PropertyInfo.Name == PropertyBindingPath);

    /// <summary>
    /// Identifies if the query string is read only (= can not be changed).
    /// </summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (_isReadOnly == value) return;
            _isReadOnly = value;
            OnPropertyChanged();
        }
    }

    private void SetQueryStringFromProperty()
    {
        if (PropertyFilter == null)
        {
            SetQueryStringInternal(string.Empty);
            IsReadOnly = false;
        }
        else
        {
            if (PropertyFilter is IPropertyFilterStringFilter propertyFilterStringFilter)
            {
                SetQueryStringInternal(propertyFilterStringFilter.FilterString ?? string.Empty);
                IsReadOnly = false;
            }
            else
            {
                var filterString = PropertyFilter.AsFilterString();

                if (filterString == null)
                {
                    SetQueryStringInternal(_queryString = FilterStringNotAvailable);
                    IsReadOnly = true;
                }
                else
                {
                    SetQueryStringInternal(_queryString = filterString);
                    IsReadOnly = false;
                }
            }
        }
    }

    private void SetQueryStringInternal(string value)
    {
        if (_queryString == value) return;
        _queryString = value;
        OnPropertyChanged(nameof(QueryString));
    }

    [Required]
    public string QueryString
    {
        get => _queryString;
        set
        {
            if (IsReadOnly)
                throw new NotSupportedException($"The property {nameof(QueryString)} cannot be changed. The query is read-only.");

            SetQueryStringInternal(value);
            UpdateFilter();
        }
    }

    private void UpdateFilter()
    {
        var propertyFilter = PropertyFilter;
        if (propertyFilter == null)
        {
            var newPropertyFilter = new PropertyFilterStringFilter(_elementType, PropertyBindingPath)
            {
                FilterString = QueryString
            };

            _dataGrid.Filter.Properties.Add(newPropertyFilter);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(QueryString))
            {
                _dataGrid.Filter.Properties.Remove(propertyFilter);
            }
            else if (propertyFilter is IPropertyFilterStringFilter propertyFilterStringFilter)
            {
                propertyFilterStringFilter.FilterString = QueryString;
            }
            else
            {
                var propertyFilterType = typeof(PropertyFilterStringFilter<>).MakeGenericType(propertyFilter.BaseType);
                var newPropertyFilter = Activator.CreateInstance(propertyFilterType, new object[]
                {
                    propertyFilter.PropertyInfo
                });

                // ReSharper disable once PossibleNullReferenceException
                typeof(IPropertyFilterStringFilter).GetProperty(nameof(IPropertyFilterStringFilter.FilterString))
                    .SetMethod.Invoke(newPropertyFilter, new object [] {QueryString});

                _dataGrid.Filter.ReplacePropertyFilter(propertyFilter, (IPropertyFilter)newPropertyFilter);
            }
        }
    }

    private void PropertyFilter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IPropertyFilterStringFilter.FilterString))
        {
            QueryString = ((IPropertyFilterStringFilter)PropertyFilter).FilterString;
        }
    }

    private void PropertyFilter_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IPropertyFilterStringFilter.FilterString))
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(QueryString)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
        
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region INotifyDataErrorInfo

    IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
    {
        if (propertyName == nameof(QueryString) && PropertyFilter is PropertyFilter)
            return (PropertyFilter as INotifyDataErrorInfo)?.GetErrors(nameof(IPropertyFilterStringFilter.FilterString));
            
        return null;
    }

    bool INotifyDataErrorInfo.HasErrors => PropertyFilter is PropertyFilter && ((PropertyFilter as INotifyDataErrorInfo)?.HasErrors ?? false);

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    #endregion
}