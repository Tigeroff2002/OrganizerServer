namespace Models.UserActionModels.NotificationModels;

public record class UserNotificationInfo
{
    public string Subject { get; }

    public string Description { get; }

    public string UserName { get; }

    public UserNotificationInfo(
        string subject,
        string description,
        string userName)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject should not be empty or white-spaces");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description should not be empty or white-spaces");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name should not be empty or white-spaces");
        }

        Subject = subject;
        Description = description;
        UserName = userName;
    }
}
