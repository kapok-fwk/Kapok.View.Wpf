using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;

namespace Kapok.View.Wpf;

public class QueryableCollectionViewSource<T> : IQueryableView<T>
    where T : class
{
    public QueryableCollectionViewSource()
    {
        ViewSource = new CollectionViewSource();
    }

    private IQueryable<T>? _queryableSource;
    public IQueryable<T>? QueryableSource
    {
        get => _queryableSource;
        set
        {
            if (_queryableSource == value) return;
            _queryableSource = value;
            Refresh();
        }
    }

    protected CollectionViewSource ViewSource { get; }

    public ICollectionView View => ViewSource.View;

    public void Refresh()
    {
        ViewSource.Source = new ObservableCollection<T>(
            QueryableSource
                .AsNoTracking() // these entries will never be changed, therefore load them as not tracked.
        );
    }
}

// TODO: rethink structure if we should keep a non-generic one. It doesn't hurt tho...
public class QueryableCollectionViewSource : QueryableCollectionViewSource<object>
{
}