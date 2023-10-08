using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public class UserCalendarSnapshotRequest : RequestWithToken
{
    [JsonProperty("user_id", Required = Required.Always, Order = 1)]
    public required int UserId { get; init; }

    [JsonProperty("begin_time", Required = Required.Always)]
    public required DateTimeOffset BeginTime { get; init; }

    [JsonProperty("end_time", Required = Required.Always)]
    public required DateTimeOffset EndTime { get; init; }
}
