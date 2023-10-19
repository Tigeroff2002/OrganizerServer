using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class UserTask
{
    [Key]
    public int Id { get; set; }

    public string Caption { get; set; }

    public string Description { get; set; }

    public TaskType TaskType { get; set; }

    public virtual User Reporter { get; set; }

    public virtual User Implementer { get; set; }

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
