using Models.BusinessModels;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class UserInfoRequest : RequestWithToken
{
    public UserInfoById ToModel()
    {
        return new UserInfoById(UserId, Token);
    }
}
