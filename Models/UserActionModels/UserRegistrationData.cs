namespace Models.BusinessModels;

public sealed class UserRegistrationData
{
    public string Email { get; }

    public string FirebaseToken { get; }

    public string UserName { get; }

    public string Password { get; }

    public string PhoneNumber { get; }

    public UserRegistrationData(
        string email, 
        string firebaseToken,
        string userName,
        string password, 
        string phoneNumber)
    {
        Email = email;
        FirebaseToken = firebaseToken;
        UserName = userName;
        Password = password;
        PhoneNumber = phoneNumber;
    }
}
