using Models.BusinessModels;
using Models.UserActionModels;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class UserLogoutDeviceByIdRequest : RequestWithToken
{
    [JsonProperty("firebase_token", Required = Required.Always)]
    public required string FirebaseToken { get; init; }

    public UserLogoutDeviceById ToModel()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            throw new ArgumentException(nameof(Token));
        }

        if (string.IsNullOrWhiteSpace(FirebaseToken))
        {
            throw new ArgumentException(nameof(FirebaseToken));
        }

        return new UserLogoutDeviceById(
            UserId,
            Token,
            FirebaseToken);
    }
}
