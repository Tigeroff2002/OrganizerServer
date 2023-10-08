using Models.BusinessModels;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class UserInfoRequest : RequestWithToken
{
    [JsonProperty("user_id", Required = Required.Always, Order = 1)]
    public required int UserId { get; init; }

    public UserInfoById ToModel()
    {
        return new UserInfoById(UserId, Token);
    }
}
