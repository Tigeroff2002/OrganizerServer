using Models.Enums;

namespace Models.BusinessModels;
public sealed class EventDomain
{
    public string Caption { get; }

    public string Description { get; }

    public DateTimeOffset ScheduledStart { get; }

    public TimeSpan Duration { get; }

    public EventType EventType { get; }

    public ActivityKind ActivityKind { get; }

    public int ManagerId { get; }

    public List<int> GuestsIds { get; }

    public EventDomain(
        string caption,
        string description,
        DateTimeOffset scheduledStart, 
        TimeSpan duration, 
        EventType eventType,
        ActivityKind activityKind,
        int managerId,
        List<int> guestsIds)
    {
        Caption = caption;
        Description = description;
        ScheduledStart = scheduledStart;
        Duration = duration;
        EventType = eventType;
        ActivityKind = activityKind;
        ManagerId = managerId;
        GuestsIds = guestsIds 
            ?? throw new ArgumentNullException(nameof(guestsIds));
    }
}
