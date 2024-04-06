namespace Models.UserActionModels;

public record class UserNotificationInfo
{
    public string Subject { get; }

    public string UserName { get; }

    public UserNotificationInfo(
        string subject,
        string userName)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject should not be empty or white-spaces");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name should not be empty or white-spaces");
        }

        Subject = subject;
        UserName = userName;
    }
}
