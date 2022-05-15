using System;
using System.ComponentModel.DataAnnotations;
using Kapok.Entity;

namespace ToDoWpfApp.DataModel;

[Display(Name = $"{nameof(TaskList)}_EntityName")]
public class TaskList : EditableEntityBase
{
    private Guid _id;
    private string _name;
    private bool _isArchived;

    [Key]
    [Display(Name = nameof(Id))]
    public Guid Id
    {
        get => _id;
        set => SetValidateProperty(ref _id, value);
    }

    [Display(Name = nameof(Name))]
    [LookupColumn]
    [Required(AllowEmptyStrings = false)]
    public string Name
    {
        get => _name;
        set => SetValidateProperty(ref _name, value);
    }

    [Display(Name = nameof(IsArchived))]
    public bool IsArchived
    {
        get => _isArchived;
        set => SetValidateProperty(ref _isArchived, value);
    }

    public override string ToString()
    {
        // Overriding ToString() is not necessary but recommended
        return $"Task list {Name}";
    }
}