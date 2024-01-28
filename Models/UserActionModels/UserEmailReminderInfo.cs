namespace Models.UserActionModels;

public sealed record class UserEmailReminderInfo
    : UserReminderInfo
{
    public string Email { get; }

    public UserEmailReminderInfo(
        string subject,
        string eventName,
        string userName,
        string email,
        int totalMinutes)
        : base(subject, eventName, userName, totalMinutes)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email should not be empty or white-spaces");
        }

        Email = email;
    }
}
