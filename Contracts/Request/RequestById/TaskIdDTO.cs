using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public class TaskIdDTO : RequestWithToken
{
    [JsonProperty("task_id", Required = Required.Always)]
    public required int TaskId { get; init; }
}
