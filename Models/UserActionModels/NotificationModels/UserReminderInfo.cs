namespace Models.UserActionModels.NotificationModels;

public record class UserReminderInfo
    : UserNotificationInfo
{
    public int TotalMinutes { get; }

    public UserReminderInfo(
        string subject,
        string description,
        string userName,
        int totalMinutes)
        : base(subject, description, userName)
    {

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Event name should not be empty or white-spaces");
        }

        TotalMinutes = totalMinutes;
    }
}
