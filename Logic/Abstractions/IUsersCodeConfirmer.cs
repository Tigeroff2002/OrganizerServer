namespace Logic.Abstractions;

public interface IUsersCodeConfirmer
{
    Task<bool> ConfirmAsync(string userEmail, CancellationToken token);
}
