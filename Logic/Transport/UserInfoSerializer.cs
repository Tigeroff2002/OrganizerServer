using Contracts.Response;

using Logic.Transport.Abstractions;
using Models;
using Newtonsoft.Json;

namespace Logic.Transport;

public sealed class UserInfoSerializer
    : ISerializer<UserInfoContent>
{
    public string Serialize(UserInfoContent entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = FromModel(entity);

        return JsonConvert.SerializeObject(dto);
    }

    public static UserInfoResponse FromModel(UserInfoContent user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserInfoResponse
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Password = user.Password,
            UserEmail = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            AccountCreationTime = user.AccountCreationTime,
            UserGroups = user.Groups,
            UserEvents = user.Events,
            UserTasks = user.Tasks,
            UserSnapshots = user.Snapshots,
            UserIssues = user.Issues,
        };
    }
}
