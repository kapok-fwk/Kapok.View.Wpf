using System;
using System.Collections.Generic;
using Kapok.BusinessLayer;
using Kapok.Data;
using ToDoWpfApp.DataModel;

namespace ToDoWpfApp.BusinessLogic;

public class TaskService : EntityDeferredCommitService<Task>
{
    public TaskService(IDataDomainScope dataDomainScope, IRepository<Task> repository) : base(dataDomainScope, repository)
    {
    }

    public override void Init(Task entry)
    {
        base.Init(entry);
        entry.Id = Guid.NewGuid();
    }


    public override bool ValidateProperty(Task entry, string propertyName, object? value, out ICollection<string> validationErrors)
    {
        var valid = base.ValidateProperty(entry, propertyName, value, out validationErrors);

        return valid;
    }
}