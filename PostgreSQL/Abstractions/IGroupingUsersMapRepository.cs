﻿using Models;
using Models.StorageModels;

namespace PostgreSQL.Abstractions;

public interface IGroupingUsersMapRepository : IRepository
{
    Task AddAsync(GroupingUsersMap map, CancellationToken token);

    Task<GroupingUsersMap?> GetGroupingUserMapByIdsAsync(
        int groupId, 
        int userId,
        CancellationToken token);

    Task<IList<GroupingUsersMap>> GetGroupingUsersMapByGroupIdsAsync(
        int groupId,
        CancellationToken token);

    Task<List<GroupingUsersMap>> GetAllMapsAsync(CancellationToken token);

    Task DeleteAsync(int groupId, int userId, CancellationToken token);
}
