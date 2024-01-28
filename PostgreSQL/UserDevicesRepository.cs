using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class UserDevicesRepository
    : IUserDevicesRepository
{
    public UserDevicesRepository(
        ILogger<UserDevicesRepository> logger,
        IServiceProvider provider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("User devices repository was created just now");
    }

    public async Task AddAsync(UserDeviceMap map, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(map);

        token.ThrowIfCancellationRequested();

        var existedMap = await _repositoryContext.UserDevices
            .FirstOrDefaultAsync(x => x.UserId == map.UserId && x.FirebaseToken == map.FirebaseToken);

        if (existedMap != null)
        {
            _repositoryContext.UserDevices.Entry(existedMap).State = EntityState.Deleted;
        }

        await _repositoryContext.UserDevices.AddAsync(map, token);
    }

    public async Task<List<UserDeviceMap>> GetAllDevicesMapsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.UserDevices.ToListAsync();
    }

    public async Task UpdateDeviceMapAsync(
        int userId, 
        string firebaseToken, 
        bool activeStatus,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var existedMap = await _repositoryContext.UserDevices.FirstOrDefaultAsync(
            x => x.UserId == userId && x.FirebaseToken == firebaseToken);

        if (existedMap != null)
        {
            existedMap.IsActive = activeStatus;
        }
    }

    public async Task DeleteAsync(int userId, string firebaseToken, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var localMap = await _repositoryContext.UserDevices.FirstOrDefaultAsync(
            x => x.UserId == userId && x.FirebaseToken == firebaseToken);

        if (localMap != null)
        {
            _repositoryContext.UserDevices.Entry(localMap).State = EntityState.Deleted;
        }
    }

    public void SaveChanges()
    {
        throw new NotImplementedException();
    }

    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
