using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class UsersRepository
    : IUsersRepository
{
    public UsersRepository(
        IServiceProvider provider,
        ILogger<UsersRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Users repository was created just now");
    }

    public async Task AddAsync(User user, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(user);

        token.ThrowIfCancellationRequested();

        await _repositoryContext.Users.AddAsync(user, token);
    }

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Users.FindAsync(
            new object?[]
            {
                userId
            }, token);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException();
        }

        return await _repositoryContext.Users
            .FirstOrDefaultAsync(x => x.Email.Equals(email));
    }

    public Task<List<User>> GetAllUsersAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return _repositoryContext.Users.ToListAsync(token);
    }

    public async Task DeleteAsync(int id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var user = await _repositoryContext.Users.FindAsync(
            new object?[]
            {
                id
            }, token);

        if (user != null)
        {
            _repositoryContext.Users.Entry(user).State = EntityState.Deleted;
        }
    }

    public async Task UpdateAsync(User user, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(user);

        token.ThrowIfCancellationRequested();

        var localUser = await _repositoryContext.Users.FindAsync(
            new object?[]
            {
                user.Id
            }, token);

        if (localUser != null)
        {
            _repositoryContext.Users.Entry(localUser).CurrentValues.SetValues(user);
        }
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of users were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
