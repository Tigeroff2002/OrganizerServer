using Contracts.Response;
using Models.Enums;

namespace Logic.Abstractions; 

public interface IReportsHandler
{
    public Task<ReportDescriptionResult> CreateReportDescriptionAsync(
        int userId,
        ReportType reportType,
        CancellationToken token);
}
