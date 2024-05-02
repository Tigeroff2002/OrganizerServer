using Models.Enums;
using Models;
using Models.StorageModels;

namespace PostgreSQL.Abstractions;

public interface IUserDevicesRepository : IRepository
{
    Task AddAsync(UserDeviceMap map, CancellationToken token);

    Task<List<UserDeviceMap>> GetAllDevicesMapsAsync(CancellationToken token);

    Task UpdateDeviceMapAsync(
        int userId,
        string firebaseToken,
        bool activeStatus,
        CancellationToken token);

    Task DeleteAsync(int userId, string firebaseToken, CancellationToken token);
}
