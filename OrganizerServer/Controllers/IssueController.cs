using Contracts;
using Contracts.Request;
using Contracts.Request.RequestById;
using Contracts.Response;
using Logic.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.BusinessModels;
using Models.Enums;
using Models.StorageModels;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using static Google.Apis.Requests.BatchRequest;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("issues")]
public sealed class IssueController : ControllerBase
{
    public IssueController(IIssuesHandler issuesHandler)
    {
        _issuesHandler = issuesHandler
            ?? throw new ArgumentNullException(nameof(issuesHandler));
    }

    [HttpPost]
    [Route("create_new")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateIssue(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _issuesHandler.TryCreateIssue(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("update_issue_params")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateIssueParams(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _issuesHandler.TryUpdateIssue(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("delete_issue")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteIssue(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _issuesHandler.TryDeleteIssue(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("get_issue_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetIssueInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _issuesHandler.GetIssueInfo(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("get_all_issues")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetAllIssues(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _issuesHandler.GetAllIssuesInfo(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    private readonly IIssuesHandler _issuesHandler;
}
