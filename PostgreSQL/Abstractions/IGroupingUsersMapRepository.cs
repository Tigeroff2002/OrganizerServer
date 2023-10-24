using Models;

namespace PostgreSQL.Abstractions;

public interface IGroupingUsersMapRepository : IRepository
{
    Task AddAsync(GroupingUsersMap map, CancellationToken token);

    Task DeleteAsync(int groupId, int userId, CancellationToken token);
}
