using Models.Enums;

namespace Models;

public class UserTask
{
    public int Id { get; set; }

    public string Caption { get; }

    public string Description { get; }

    public TaskType TaskType { get; }

    public virtual User Reporter { get; }

    public int ReporterId { get; }

    public virtual User Implementer { get; }

    public int ImplementerId { get; }   

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
