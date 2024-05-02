using Models;
using Models.Enums;
using Models.StorageModels;

namespace PostgreSQL.Abstractions;

public interface IUsersRepository
    : IRepository
{
    Task AddAsync(User user, CancellationToken token);

    Task<User?> GetUserByIdAsync(int userId, CancellationToken token);

    Task<User?> GetUserByEmailAsync(string email, CancellationToken token);

    Task<List<User>> GetAllUsersAsync(CancellationToken token);

    Task DeleteAsync(int id, CancellationToken token);

    Task UpdateAsync(User user, CancellationToken token);

    Task UpdateRoleAsync(int userId, UserRole newRole, CancellationToken token);
}
