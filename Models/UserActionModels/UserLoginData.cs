namespace Models.BusinessModels;

public sealed class UserLoginData
{
    public string Email { get; }

    public string FirebaseToken { get; }

    public string Password { get; }

    public UserLoginData(
        string email,
        string firebaseToken,
        string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(nameof(email));
        }

        if (string.IsNullOrWhiteSpace(firebaseToken))
        {
            throw new ArgumentException(nameof(firebaseToken));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(nameof(password));
        }

        Email = email;
        FirebaseToken = firebaseToken;
        Password = password;
    }
}
