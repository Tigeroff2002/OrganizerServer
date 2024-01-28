using Contracts.Request.RequestById;
using Logic.Transport.Abstractions;
using Models.BusinessModels;
using Models.UserActionModels;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Logic.Transport;

public sealed class UserLogoutDeviceByIdDeserializer
    : IDeserializer<UserLogoutDeviceById>
{
    public UserLogoutDeviceById Deserialize(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(nameof(source));
        }

        var dto = JsonConvert.DeserializeObject<UserLogoutDeviceByIdRequest>(source);

        Debug.Assert(dto != null);

        return new(dto.UserId, dto.Token, dto.FirebaseToken);
    }
}
