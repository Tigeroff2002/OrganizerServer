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

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("reports")]
public sealed class ReportController : ControllerBase
{
    public ReportController(
        IReportsRepository reportsRepository,
        IUsersRepository usersRepository)
    {
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

        var user = await _usersRepository.GetUserByIdAsync(reportToCreate.UserId, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"Report has not been created cause current user was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var report = new Report
        {
            ReportType = reportToCreate.ReportType,
            BeginMoment = reportToCreate.BeginMoment,
            EndMoment = reportToCreate.EndMoment,
            Description = string.Empty,
            UserId = user.Id
        };

        await _reportsRepository.AddAsync(report, token);

        var reportId = report.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New report with id = {reportId}" +
            $" for user '{user.UserName}' was created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [HttpDelete]
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

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Report with id {reportId} was deleted by reporter";
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such report with id {reportId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpGet]
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

            var reporterInfo = new ShortUserInfo
            {
                UserEmail = reporter.Email,
                UserName = reporter.UserName,
                UserPhone = reporter.PhoneNumber
            };

            var reportInfo = new ReportInfoResponse
            {
                ReportDescription = existedReport.Description,
                ReportType = existedReport.ReportType,
                BeginMoment = existedReport.BeginMoment,
                EndMoment = existedReport.EndMoment,
                Reporter = reporterInfo
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

    private readonly IReportsRepository _reportsRepository;
    private readonly IUsersRepository _usersRepository;
}

