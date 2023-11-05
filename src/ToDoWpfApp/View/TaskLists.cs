using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.View;
using ToDoWpfApp.DataModel;

namespace ToDoWpfApp.View;

public class TaskLists : ListPage<TaskList>
{
    public TaskLists(IViewDomain? viewDomain = null, IDataDomain? dataDomain = null)
        : base(viewDomain, dataDomain)
    {
        Title = "Task lists";

        ListViews.Add(new DataSetListView
        {
            Name = "Standard",
            Columns = new List<ColumnPropertyView>
            {
                new(nameof(TaskList.Name))
            }
        });

        OpenTasksAction = new UIOpenReferencedPageAction<TaskList>("OpenTasks", typeof(Tasks), ViewDomain, DataSet,
            filter: (filter, taskList, _) =>
            {
                var filterSet = (IFilterSet<Task>)filter;
                filterSet.AddPropertyFilter(nameof(Task.TaskListId), taskList.Id);
            });
    }

    [MenuItem, Display(Name = "Tasks")]
    public IDataSetSelectionAction<TaskList> OpenTasksAction { get; }
}