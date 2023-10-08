using Models;

namespace PostgreSQL.Abstractions;

public interface IGroupsRepository : IRepository
{
    Task AddAsync(Group group, CancellationToken token);

    Task<Group?> GetGroupByIdAsync(int groupId, CancellationToken token);

    Task<List<Group>> GetAllGroupsAsync(CancellationToken token);

    Task DeleteAsync(int groupId, CancellationToken token);

    Task UpdateAsync(Group group, CancellationToken token);
}
