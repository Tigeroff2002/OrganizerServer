using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public class UserCalendarSnapshotRequest : RequestWithToken
{
    [JsonProperty("begin_time", Required = Required.Always)]
    public required DateTimeOffset BeginTime { get; init; }

    [JsonProperty("end_time", Required = Required.Always)]
    public required DateTimeOffset EndTime { get; init; }
}
