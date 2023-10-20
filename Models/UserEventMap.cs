namespace Models;

public class UserEventMap
{
    public int UserId { get; set; }

    public User User { get; set; }

    public int EventId { get; set; }

    public Event Event { get; set; }
}
