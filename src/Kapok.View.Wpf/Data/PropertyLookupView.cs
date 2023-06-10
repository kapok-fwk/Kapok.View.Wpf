using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Kapok.Data;
using Kapok.Entity.Model;

namespace Kapok.View.Wpf;

/// <summary>
/// A property lookup view holds the information what items to show in a UI combobox. 
/// </summary>
public class PropertyLookupView : IPropertyLookupView
{
    private readonly Func<object?>? _currentSelector;
    private readonly IDataDomain _dataDomain;
    private bool _isRefreshedOnce;

    /// <summary>
    /// Constructs a new PropertyLookupView which handles and holds the possible items to show in a UI combobox.
    /// </summary>
    /// <param name="lookupDefinition">
    /// The lookup definition for the property.
    /// </param>
    /// <param name="dataDomain">
    /// The data domain which is used in case database interactions are required while creating the list of possible items.
    /// </param>
    /// <param name="currentSelector">
    /// A selector returning the current item or <c>null</c>.
    /// Sometimes property lookups depend on other properties of the current instance of an object.
    /// </param>
    public PropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain,
        Func<object?>? currentSelector = null)
    {
        LookupDefinition = lookupDefinition;
        _dataDomain = dataDomain;
        _currentSelector = currentSelector;
    }

    [Obsolete("Please use constructor PropertyLookupView(ILookupDefinition, IDataDomain, Func<object>) instead.", true)]
    public PropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain, IDataSetView? dataSetView = null)
    {
        LookupDefinition = lookupDefinition;
        _dataDomain = dataDomain;
        _currentSelector = () => dataSetView?.Current;
    }

    public ILookupDefinition LookupDefinition { get; }

    protected CollectionViewSource ViewSource { get; } = new();

    /// <summary>
    /// This property is used in WPF to access the view object.
    /// 
    /// At the first call a refresh is enforced.
    /// </summary>
    public ICollectionView View
    {
        get
        {
            if (!_isRefreshedOnce)
                Refresh();

            return ViewSource.View;
        }
    }

    public void Refresh()
    {
        object? newSource;

        if (LookupDefinition.EntriesFuncDependentOnEntry)
        {
            var current = _currentSelector?.Invoke();
            if (current == null)
            {
                newSource = null; // no data...
            }
            else
            {
                using var scope = _dataDomain.CreateScope();
                newSource = new ObservableCollection<object>(LookupDefinition.EntriesFunc.Invoke(current, scope));
            }
        }
        else
        {
            using var scope = _dataDomain.CreateScope();
            newSource = new ObservableCollection<object>(LookupDefinition.EntriesFunc.Invoke(null, scope));
        }

        ViewSource.Source = newSource;
        _isRefreshedOnce = true;
    }
}