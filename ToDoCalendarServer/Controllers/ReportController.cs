using Contracts.Response;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models.BusinessModels;
using Models;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using Contracts.Request;
using Microsoft.AspNetCore.Authorization;
using Logic.Abstractions;
using Models.Enums;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("reports")]
public sealed class ReportController : ControllerBase
{
    public ReportController(
        IReportsHandler reportsHandler,
        IReportsRepository reportsRepository,
        IUsersRepository usersRepository)
    {
        _reportsHandler = reportsHandler
            ?? throw new ArgumentNullException(nameof(reportsHandler));

        _reportsRepository = reportsRepository 
            ?? throw new ArgumentNullException(nameof(reportsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));
    }

    [HttpPost]
    [Route("perform_new")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateReport(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var reportToCreate = JsonConvert.DeserializeObject<ReportInputDTO>(body);

        Debug.Assert(reportToCreate != null);

        var userId = reportToCreate.UserId;
        var reportType = reportToCreate.ReportType;

        var user = await _usersRepository.GetUserByIdAsync(userId, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"Report has not been created cause current user was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var reportDescriptionResult = await _reportsHandler
            .CreateReportDescriptionAsync(userId, reportType, token);

        Debug.Assert(reportDescriptionResult != null);

        var description = JsonConvert.SerializeObject(reportDescriptionResult);

        var report = new Report
        {
            Description = description,
            ReportType = reportToCreate.ReportType,
            BeginMoment = reportToCreate.BeginMoment,
            EndMoment = reportToCreate.EndMoment,
            UserId = user.Id
        };

        await _reportsRepository.AddAsync(report, token);

        _reportsRepository.SaveChanges();

        var reportId = report.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New report with id = {reportId}" +
            $" for user '{user.UserName}'" +
            $" with type '{reportType}' has been created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [Route("delete_report")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteReport(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var reportToDelete = JsonConvert.DeserializeObject<ReportIdDTO>(body);

        Debug.Assert(reportToDelete != null);

        var reportId = reportToDelete.ReportId;

        var existedReport = await _reportsRepository.GetReportByIdAsync(reportId, token);

        if (existedReport != null)
        {
            var userId = existedReport!.UserId;

            if (reportToDelete.UserId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Report has not been deleted cause user is not its reporter";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            await _reportsRepository.DeleteAsync(reportId, token);

            _reportsRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Report with id {reportId} was deleted by reporter";

            return Ok(JsonConvert.SerializeObject(response));
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such report with id {reportId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_report_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetGroupInfo(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var reportWithIdRequest = JsonConvert.DeserializeObject<ReportIdDTO>(body);

        Debug.Assert(reportWithIdRequest != null);

        var reportId = reportWithIdRequest.ReportId;

        var existedReport = await _reportsRepository.GetReportByIdAsync(reportId, token);

        if (existedReport != null)
        {
            var userId = existedReport!.UserId;

            var reporter = await _usersRepository.GetUserByIdAsync(userId, token);

            if (reporter == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Cant take info about report cause user with id {userId} is not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (reportWithIdRequest.UserId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Cant take info about report cause user is not its reporter";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var reportDescription = existedReport.Description;

            ReportDescriptionResult? descriptionModel = 
                existedReport.ReportType == ReportType.TasksReport
                    ? JsonConvert.DeserializeObject<ReportTasksDescriptionResult>(reportDescription)
                    : JsonConvert.DeserializeObject<ReportEventsDescriptionResult>(reportDescription);

            Debug.Assert(descriptionModel != null);

            var reportInfo = new ReportInfoResponse
            {
                BeginMoment = existedReport.BeginMoment,
                EndMoment = existedReport.EndMoment,
                ReportType = existedReport.ReportType,
                ReportContent = descriptionModel
            };

            var getReponse = new GetResponse();
            getReponse.Result = true;
            getReponse.OutInfo = $"Info about report with id {reportId} for user with id {userId} was received";
            getReponse.RequestedInfo = reportInfo;

            var json = JsonConvert.SerializeObject(getReponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such report with id {reportId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);

        return await reader.ReadToEndAsync();
    }

    private readonly IReportsHandler _reportsHandler;
    private readonly IReportsRepository _reportsRepository;
    private readonly IUsersRepository _usersRepository;
}

