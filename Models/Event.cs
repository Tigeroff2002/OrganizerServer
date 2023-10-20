using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class Event
{
    [Key]
    public int Id { get; set; }

    public string Caption { get; }

    public string Description { get; set; }

    public DateTimeOffset ScheduledStart { get; }

    public TimeSpan Duration { get; set; }

    public EventType EventType { get; set; }

    public ActivityKind ActivityKind { get; set; }

    public virtual User Manager { get; set; }

    public virtual List<UserEventMap> UserEventMaps { get; set; }

    public Event() { }
}
