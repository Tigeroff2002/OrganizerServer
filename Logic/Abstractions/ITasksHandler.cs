using Models.BusinessModels;

namespace Logic.Abstractions;

public interface ITasksHandler
{
    public Task<Response> TryCreateTask(
        string createData,
        CancellationToken token);

    public Task<Response> TryUpdateTask(
        string updateData,
        CancellationToken token);

    public Task<Response> TryDeleteTask(
        string deleteData,
        CancellationToken token);

    public Task<GetResponse> GetTaskInfo(
        string taskInfoById,
        CancellationToken token);
}
