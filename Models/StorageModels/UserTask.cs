using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.StorageModels;

public class UserTask
{
    [Key]
    public int Id { get; set; }

    public string Caption { get; set; }

    public string Description { get; set; }

    public TaskType TaskType { get; set; }

    public TaskCurrentStatus TaskStatus { get; set; }

    public virtual User Reporter { get; set; }

    public int ReporterId { get; set; }

    public virtual User Implementer { get; set; }

    public int ImplementerId { get; set; }

    public UserTask() { }
}
