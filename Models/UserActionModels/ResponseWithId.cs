using Models.BusinessModels;
using Newtonsoft.Json;

namespace Models.UserActionModels;

public sealed class ResponseWithId : Response
{
    [JsonProperty("id", Required = Required.Always)]
    public required int Id { get; init; }
}
