using Models;
using Models.StorageModels;

namespace PostgreSQL.Abstractions;

public interface ITasksRepository : IRepository
{
    Task AddAsync(UserTask task, CancellationToken token);

    Task<UserTask?> GetTaskByIdAsync(int taskId, CancellationToken token);

    Task<List<UserTask>> GetAllTasksAsync(CancellationToken token);

    Task DeleteAsync(int taskId, CancellationToken token);

    Task UpdateAsync(UserTask task, CancellationToken token);
}
