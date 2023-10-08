using Contracts;
using Logic.Transport.Abstractions;
using Models.BusinessModels;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Logic.Transport;

public sealed class UsersLoginDataDeserializer
    : IDeserializer<UserLoginData>
{
    public UserLoginData Deserialize(string source)
    {
        var dto = JsonConvert.DeserializeObject<UserLoginDataDTO>(source);

        Debug.Assert(dto != null);

        return dto!.ToModel();
    }
}
