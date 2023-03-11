using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Kapok.Data;
using Kapok.Entity.Model;

namespace Kapok.View.Wpf;

public class PropertyLookupView : IPropertyLookupView
{
    private readonly IDataSetView? _dataSetView;
    private readonly IDataDomain _dataDomain;
    private bool _isRefreshedOnce;

    public PropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain, IDataSetView? dataSetView = null)
    {
        _dataSetView = dataSetView;
        LookupDefinition = lookupDefinition;
        _dataDomain = dataDomain;

        ViewSource = new CollectionViewSource();
    }

    public ILookupDefinition LookupDefinition { get; }

    protected CollectionViewSource ViewSource { get; }

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
            if (_dataSetView?.Current == null)
            {
                newSource = null; // no data...
            }
            else
            {
                using var scope = _dataDomain.CreateScope();
                newSource = new ObservableCollection<object>(LookupDefinition.EntriesFunc.Invoke(_dataSetView.Current, scope));
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