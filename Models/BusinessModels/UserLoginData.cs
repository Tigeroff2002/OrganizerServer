namespace Models.BusinessModels;

public sealed class UserLoginData
{
    public string Email { get; }

    public string Password { get; }

    public UserLoginData(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
