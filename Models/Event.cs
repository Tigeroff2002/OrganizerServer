using Models.Enums;

namespace Models;

public class Event
{
    public int Id { get; set; }

    public string Caption { get; }

    public string Description { get; set; }

    public DateTimeOffset ScheduledStart { get; }

    public TimeSpan Duration { get; }

    public EventType EventType { get; }

    public ActivityKind ActivityKind { get; }

    public virtual User Manager { get; set; }

    public int ManagerId { get; }

    public virtual List<User> Guests { get; set; }

    public Event(
        int id,
        string caption,
        string description,
        DateTimeOffset scheduledStart, 
        TimeSpan duration, 
        EventType eventType)
    {
        Id = id;
        Caption = caption;
        Description = description;
        ScheduledStart = scheduledStart;
        Duration = duration;
        EventType = eventType;
    }
}
