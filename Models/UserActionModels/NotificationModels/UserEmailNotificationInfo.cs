namespace Models.UserActionModels.NotificationModels;

public sealed record class UserEmailNotificationInfo : UserNotificationInfo
{
    public string Email { get; }

    public UserEmailNotificationInfo(
        string subject, 
        string description, 
        string userName,
        string email) 
        : base(subject, description, userName)
    {
        Email = email;
    }
}
