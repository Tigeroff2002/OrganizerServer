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

        _logger.LogInformation("Users repository was created just now");
    }

    public async Task AddAsync(User user, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(user);

        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        await _repositoryContext.Users.AddAsync(user, token);

        SaveChanges();
    }

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

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

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        return await _repositoryContext.Users
            .FirstOrDefaultAsync(x => x.Email.Equals(email));
    }

    public Task<List<User>> GetAllUsersAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        return _repositoryContext.Users.ToListAsync(token);
    }

    public async Task DeleteAsync(int id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var user = await _repositoryContext.Users.FindAsync(
            new object?[]
            {
                id
            }, token);

        if (user != null)
        {
            _repositoryContext.Users.Entry(user).State = EntityState.Deleted;
        }

        SaveChanges();
    }

    public async Task UpdateAsync(User user, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(user);

        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var localUser = await _repositoryContext.Users.FindAsync(
            new object?[]
            {
                user.Id
            }, token);

        if (localUser != null)
        {
            _repositoryContext.Users.Entry(localUser).CurrentValues.SetValues(user);
        }

        SaveChanges();
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of users were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
