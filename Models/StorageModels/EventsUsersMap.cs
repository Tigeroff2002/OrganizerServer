using Models.Enums;

namespace Models.StorageModels;

public class EventsUsersMap
{
    public int UserId { get; set; }

    public User User { get; set; }

    public DecisionType DecisionType { get; set; }

    public int EventId { get; set; }

    public Event Event { get; set; }
}
