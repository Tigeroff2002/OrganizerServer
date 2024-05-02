using Contracts;
using Logic.Transport.Abstractions;
using Models.BusinessModels;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Logic.Transport.Serialization;

public sealed class UsersRegistrationDataDeserializer
    : IDeserializer<UserRegistrationData>
{
    public UserRegistrationData Deserialize(string source)
    {
        var dto = JsonConvert.DeserializeObject<UserRegistrationDataDTO>(source);

        Debug.Assert(dto != null);

        return dto!.ToModel();
    }
}
