using Contracts.Response;

using Logic.Transport.Abstractions;
using Models;
using Newtonsoft.Json;

namespace Logic.Transport;

public sealed class UserInfoSerializer
    : ISerializer<User>
{
    public string Serialize(User entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = FromModel(entity);

        return JsonConvert.SerializeObject(dto);
    }

    public static UserInfoResponse FromModel(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new()
        {
            UserId = user.Id,
            UserName = user.UserName,
            UserEmail = user.Email,
            UserToken = user.AuthToken,
            UserGroups = new List<GroupInfoResponse>()
            {

            },
            UserTasks = new List<TaskInfoResponse>()
            {

            },
            UserEvents = new List<EventInfoResponse>()
            {

            },
            UserReports = new List<ReportInfoResponse>()
            {

            }
        };
    }
}
