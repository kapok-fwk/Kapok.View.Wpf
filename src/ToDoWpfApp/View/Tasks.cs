using System;
using System.Collections.Generic;
using System.Threading;
using Kapok.View;
using ToDoWpfApp.DataModel;

namespace ToDoWpfApp.View;

public class Tasks : ListPage<Task>
{
    public Tasks(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Title = $"Tasks {Thread.CurrentThread.CurrentUICulture}";

        ListViews.Add(new DataSetListView
        {
            Name = "Standard",
            Columns = new List<ColumnPropertyView>
            {
                new(nameof(Task.Name)),
                new(nameof(Task.EstimatedTime)),
            }
        });
        ListViews.Add(new DataSetListView
        {
            Name = "With due date",
            Columns = new List<ColumnPropertyView>
            {
                new(nameof(Task.Name)),
                new(nameof(Task.EstimatedTime)),
                new(nameof(Task.DueDate)),
            }
        });
    }
}