namespace Models.BusinessModels;

public sealed class Notification
{
    public int EventId { get; }

    public TimeSpan RemainingTime { get; }

    public string Text { get; }

    public Notification(
        int eventId,
        TimeSpan remainingTime,
        string text)
    {
        EventId = eventId;
        RemainingTime = remainingTime;
        Text = text;
    }
}  
