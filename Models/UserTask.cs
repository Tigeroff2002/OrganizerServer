using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class UserTask
{
    [Key]
    public int Id { get; set; }

    public string Caption { get; }

    public string Description { get; }

    public TaskType TaskType { get; }

    public virtual User Reporter { get; }

    public virtual User Implementer { get; }

    public UserTask() { }

    public UserTask(
        int id,
        string caption, 
        string description,
        TaskType taskType)
    {
        Id = id;
        Caption = caption;
        Description = description;
        TaskType = taskType;
    }
}
