namespace Models.UserActionModels;

public record class UserReminderInfo
    : UserNotificationInfo
{
    public string EventName { get; }

    public int TotalMinutes { get; }

    public UserReminderInfo(
        string subject,
        string eventName,
        string userName,
        int totalMinutes)
        : base(subject, userName)
    {

        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Event name should not be empty or white-spaces");
        }

        EventName = eventName;
        TotalMinutes = totalMinutes;
    }
}
