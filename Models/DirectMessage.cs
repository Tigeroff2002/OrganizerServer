namespace Models;

public class DirectMessage
{
    public int Id { get; set; }

    public DateTimeOffset SendTime { get; set; }

    public string Text { get; set; }

    public bool isEdited { get; set; } = false;

    public int UserId { get; set; }

    public virtual User User { get; set; }

    public int ChatId { get; set; }

    public virtual DirectChat Chat { get; set; }
}
