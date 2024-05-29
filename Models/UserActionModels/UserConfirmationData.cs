namespace Models.BusinessModels;

public sealed class UserConfirmationData
{
    public string Email { get; }

    public string UserName { get; }

    public string Password { get; }

    public string PhoneNumber { get; }

    public string FirebaseToken { get; }    

    public string Code { get; }

    public DateTimeOffset StartTime { get; }

    public UserConfirmationData(
        string email,
        string userName,
        string password,
        string phoneNumber,
        string firebaseToken,
        string code)
    {
        Code = code;
        Email = email;
        UserName = userName;
        Password = password;
        PhoneNumber = phoneNumber;
        FirebaseToken = firebaseToken;
        StartTime = DateTimeOffset.UtcNow;
    }

    public bool IsActual() => DateTimeOffset.UtcNow - StartTime <= TimeSpan.FromMinutes(2);
}
