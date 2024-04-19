using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Models.Enums;
using Models.StorageModels;
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

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
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

        return await _repositoryContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
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

    public async Task<List<User>> GetAllUsersAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Users.ToListAsync(token);
    }

    public async Task DeleteAsync(int id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var user = await _repositoryContext.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user != null)
        {
            _repositoryContext.Users.Entry(user).State = EntityState.Deleted;
        }
    }

    public async Task UpdateAsync(User user, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(user);

        token.ThrowIfCancellationRequested();

        var localUser = await _repositoryContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id);

        if (localUser != null)
        {
            _repositoryContext.Users.Entry(localUser).CurrentValues.SetValues(user);
        }

        localUser = user.Map<User>();
    }

    public async Task UpdateRoleAsync(int userId, UserRole newRole, CancellationToken token)
    {
        if (userId < 0)
        {
            throw new ArgumentException($"User id {userId} should be positive");
        }

        token.ThrowIfCancellationRequested();

        var user = await _repositoryContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user != null)
        {
            _repositoryContext.Users.Entry(user).Entity.Role = newRole;
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
