using Models.BusinessModels;

namespace Logic.Abstractions;

public interface IIssuesHandler
{
    public Task<Response> TryCreateIssue(
        string createData,
        CancellationToken token);

    public Task<Response> TryUpdateIssue(
        string updateData,
        CancellationToken token);

    public Task<Response> TryDeleteIssue(
        string deleteData,
        CancellationToken token);

    public Task<GetResponse> GetIssueInfo(
        string issueInfoById,
        CancellationToken token);

    public Task<GetResponse> GetAllIssuesInfo(
        string allIssuesDTO,
        CancellationToken token);
}
