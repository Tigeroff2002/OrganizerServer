using Newtonsoft.Json;

namespace Contracts.Request.UsersListRequests;

public class AllGroupUsersDTO : AllUsersRequestDTO
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }
}
