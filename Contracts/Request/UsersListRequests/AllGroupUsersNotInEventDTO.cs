using Newtonsoft.Json;

namespace Contracts.Request.UsersListRequests;

public sealed class AllGroupUsersNotInEventDTO : AllGroupUsersDTO
{
    [JsonProperty("event_id", Required = Required.Always)]
    public required int EventId { get; init; }
}
