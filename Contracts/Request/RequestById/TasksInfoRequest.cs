using Models.Enums;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class TasksInfoRequest : RequestWithToken
{
    [JsonProperty("user_id", Required = Required.Always, Order = 1)]
    public required int UserId { get; init; }

    [JsonProperty("task_type", Required = Required.Default)]
    public TaskType TaskType { get; init; }
}
