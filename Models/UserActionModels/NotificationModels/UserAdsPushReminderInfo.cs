namespace Models.UserActionModels.NotificationModels;

public sealed record class UserAdsPushReminderInfo
    : UserReminderInfo
{
    public string FirebaseToken { get; }

    public UserAdsPushReminderInfo(
        string subject,
        string eventName,
        string userName,
        string firebaseToken,
        int totalMinutes)
        : base(subject, eventName, userName, totalMinutes)
    {
        if (string.IsNullOrWhiteSpace(firebaseToken))
        {
            throw new ArgumentException(
                "Firebase token should not be empty or white-spaces");
        }

        FirebaseToken = firebaseToken;
    }
}
