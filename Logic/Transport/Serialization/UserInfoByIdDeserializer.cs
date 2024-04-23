using Contracts.Request.RequestById;
using Logic.Transport.Abstractions;
using Models.BusinessModels;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Logic.Transport.Serialization;

public sealed class UserInfoByIdDeserializer
    : IDeserializer<UserInfoById>
{
    public UserInfoById Deserialize(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(nameof(source));
        }

        var dto = JsonConvert.DeserializeObject<UserInfoRequest>(source);

        Debug.Assert(dto != null);

        return new(dto.UserId, dto.Token);
    }
}

