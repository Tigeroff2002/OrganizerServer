using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Response; 

public sealed class TaskInfoResponse
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

    [JsonProperty("reporter", Required = Required.Default)]
    public ShortUserInfo Reporter { get; set; } = default!;

    [JsonProperty("implementer", Required = Required.Default)]
    public ShortUserInfo Implementer { get; set; } = default!;
}
