using System.Collections.Generic;
using Kapok.Data;
using Kapok.View;
using ToDoWpfApp.DataModel;

namespace ToDoWpfApp.View;

public class Tasks : ListPage<Task>
{
    public Tasks(IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null) : base(viewDomain, dataDomainScope)
    {
        Title = "Tasks";

        ListViews.Add(new DataSetListView
        {
            Name = "Standard",
            Columns = new List<ColumnPropertyView>
            {
                new(typeof(Task).GetProperty(nameof(Task.Name)))
            }
        });
        ListViews.Add(new DataSetListView
        {
            Name = "With due date",
            Columns = new List<ColumnPropertyView>
            {
                new(typeof(Task).GetProperty(nameof(Task.Name))),
                new(typeof(Task).GetProperty(nameof(Task.DueDate))),
            }
        });
    }
}