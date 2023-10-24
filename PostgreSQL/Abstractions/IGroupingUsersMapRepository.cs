using Models;

namespace PostgreSQL.Abstractions;

public interface IGroupingUsersMapRepository : IRepository
{
    Task AddAsync(GroupingUsersMap map, CancellationToken token);

    Task<GroupingUsersMap?> GetGroupingUserMapByIdsAsync(
        int groupId, 
        int userId,
        CancellationToken token);

    Task<List<GroupingUsersMap>> GetAllMapsAsync(CancellationToken token);

    Task DeleteAsync(int groupId, int userId, CancellationToken token);
}
