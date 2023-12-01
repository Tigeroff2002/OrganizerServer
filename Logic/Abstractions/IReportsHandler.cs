using Contracts.Request;
using Contracts.Response;
using Models.Enums;

namespace Logic.Abstractions; 

public interface IReportsHandler
{
    public Task<ReportDescriptionResult> CreateReportDescriptionAsync(
        int userId,
        ReportInputDTO inputReport,
        CancellationToken token);
}
