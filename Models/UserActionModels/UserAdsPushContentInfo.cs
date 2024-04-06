using FirebaseAdmin.Auth;

namespace Models.UserActionModels;

public record class UserAdsPushContentInfo
    : UserNotificationInfo
{
    public string Content { get; }

    public string FirebaseToken { get; }

    public UserAdsPushContentInfo(
        string subject,
        string userName,
        string content,
        string firebaseToken)
        : base(subject, userName)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content should not be empty or white-spaces");
        }

        if (string.IsNullOrWhiteSpace(firebaseToken))
        {
            throw new ArgumentException(
                "Firebase token should not be empty or white-spaces");
        }

        FirebaseToken = firebaseToken;

        Content = content;
    }
}
