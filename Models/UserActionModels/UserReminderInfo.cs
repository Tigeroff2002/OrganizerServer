namespace Models.UserActionModels;

public record class UserReminderInfo
{
    public string Subject { get; }

    public string EventName { get; }

    public string UserName { get; }

    public int TotalMinutes { get; }

    public UserReminderInfo(
        string subject,
        string eventName,
        string userName,
        int totalMinutes)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject should not be empty or white-spaces");
        }

        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Event name should not be empty or white-spaces");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name should not be empty or white-spaces");
        }

        Subject = subject;
        EventName = eventName;
        UserName = userName;
        TotalMinutes = totalMinutes;
    }
}
