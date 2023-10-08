using Models.Enums;

namespace Models.BusinessModels;

public class TaskDomain
{
    public string TaskCaption { get; }

    public string TaskDescription { get; }

    public TaskType TaskType { get; }

    public int ReporterId { get; }

    public int ImplementerId { get; }

    public TaskDomain(
        string taskCaption, 
        string taskDescription, 
        TaskType taskType, 
        int reporterId,
        int implementerId)
    {
        TaskCaption = taskCaption;
        TaskDescription = taskDescription;
        TaskType = taskType;
        ReporterId = reporterId;
        ImplementerId = implementerId;
    }
}
