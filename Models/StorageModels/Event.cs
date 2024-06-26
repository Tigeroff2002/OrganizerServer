﻿using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.StorageModels;

public class Event
{
    [Key]
    public int Id { get; set; }

    public string Caption { get; set; }

    public string Description { get; set; }

    public DateTimeOffset ScheduledStart { get; set; }

    public TimeSpan Duration { get; set; }

    public EventType EventType { get; set; }

    public EventStatus Status { get; set; }

    //public RecursionEventType RecursionEventType { get; set; }

    //public string MeetLinkUrl { get; set; }

    public virtual Group RelatedGroup { get; set; }

    public int RelatedGroupId { get; set; }

    public virtual User Manager { get; set; }

    public int ManagerId { get; set; }

    public virtual List<EventsUsersMap> GuestsMap { get; set; }

    public Event() { }
}
