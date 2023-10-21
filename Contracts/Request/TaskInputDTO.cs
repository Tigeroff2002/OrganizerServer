using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Contracts.Request;

namespace Contracts;

public class TaskInputDTO : RequestWithToken
{
    [JsonProperty("caption", Required = Required.Always)]
    public required string TaskCaption { get; init; }

    [JsonProperty("description", Required = Required.Always)]
    public required string TaskDescription { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("task_type", Required = Required.Always)]
    public required TaskType TaskType { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("task_type", Required = Required.Default)]
    public required TaskCurrentStatus TaskStatus { get; init; } = TaskCurrentStatus.ToDo;

    [JsonProperty("implementer_id", Required = Required.Always)]
    public required int ImplementerId { get; init; }
}
