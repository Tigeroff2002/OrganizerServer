using Models.BusinessModels;

namespace Models.UserActionModels;

public record class UserLogoutDeviceById : UserInfoById
{
    public string FirebaseToken { get; }

    public UserLogoutDeviceById(
        int userId,
        string token,
        string firebaseToken)
        : base(userId, token)
    {
        if (string.IsNullOrWhiteSpace(firebaseToken))
        {
            throw new ArgumentException(nameof(firebaseToken));
        }

        FirebaseToken = firebaseToken;
    }
}
