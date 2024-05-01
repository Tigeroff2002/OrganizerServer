using FirebaseAdmin.Auth;

namespace Models.UserActionModels.NotificationModels;

public record class UserAdsPushContentInfo
    : UserNotificationInfo
{
    public string FirebaseToken { get; }

    public UserAdsPushContentInfo(
        string subject,
        string userName,
        string description,
        string firebaseToken)
        : base(subject, description, userName)
    {
        if (string.IsNullOrWhiteSpace(firebaseToken))
        {
            throw new ArgumentException(
                "Firebase token should not be empty or white-spaces");
        }

        FirebaseToken = firebaseToken;
    }
}
