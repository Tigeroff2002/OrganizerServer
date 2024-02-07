using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class MessageInfoResponse
{
    [JsonProperty("message_id", Required = Required.Always)]
    public required int MessageId { get; init; }

    [JsonProperty("text", Required = Required.Always)]
    public required string Text { get; init; }

    [JsonProperty("send_time", Required = Required.Always)]
    public required DateTimeOffset SendTime { get; init; }

    [JsonProperty("is_edited", Required = Required.Always)]
    public required bool IsEdited { get; init; }

    [JsonProperty("writer_id", Required = Required.Always)]
    public required int WriterId { get; init; }
}
