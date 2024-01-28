using Models.BusinessModels;

namespace Contracts.Request.RequestById;

public sealed class UserInfoRequest : RequestWithToken
{
    public UserInfoById ToModel()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            throw new ArgumentException(nameof(Token));
        }

        return new UserInfoById(UserId, Token);
    }
}
