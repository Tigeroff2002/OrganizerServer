using Models.BusinessModels;
using Newtonsoft.Json;

namespace Contracts;

public sealed class UserRegistrationDataDTO
{
    [JsonProperty("email", Required = Required.Always)]
    public required string Email { get; init; }

    [JsonProperty("name", Required = Required.Always)]
    public required string UserName { get; init; }

    [JsonProperty("password", Required = Required.Always)]
    public required string Password { get; init; }

    [JsonProperty("phone_number", Required = Required.Always)]
    public required string PhoneNumber { get; init; }

    public UserRegistrationData ToModel()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            throw new ArgumentException(nameof(Email));
        }

        if (string.IsNullOrWhiteSpace(UserName))
        {
            throw new ArgumentException(nameof(UserName));
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            throw new ArgumentException(nameof(Password));
        }

        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            throw new ArgumentException(nameof(PhoneNumber));
        }

        return new(
            Email,
            UserName, 
            Password, 
            PhoneNumber);
    }
}
