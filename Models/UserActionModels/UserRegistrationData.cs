namespace Models.BusinessModels;

public sealed class UserRegistrationData
{
    public string Email { get; }

    public string UserName { get; }

    public string Password { get; }

    public string PhoneNumber { get; }

    public UserRegistrationData(
        string email, 
        string userName,
        string password, 
        string phoneNumber)
    {
        Email = email;
        UserName = userName;
        Password = password;
        PhoneNumber = phoneNumber;
    }
}
