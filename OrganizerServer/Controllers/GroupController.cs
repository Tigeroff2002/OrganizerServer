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

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("groups")]
public sealed class GroupController : ControllerBase
{
    public GroupController(
        IGroupsHandler groupsHandler) 
    {
        _groupsHandler = groupsHandler 
            ?? throw new ArgumentNullException(nameof(groupsHandler));
    }

    [HttpPost]
    [Route("create")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateGroup(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _groupsHandler.TryCreateGroup(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("update_group_params")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateGroupParams(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _groupsHandler.TryUpdateGroup(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("delete_participant")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteParticipant(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _groupsHandler.TryDeleteParticipant(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("delete_group")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetGroupInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _groupsHandler.GetGroupInfo(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("delete_group")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteGroup(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _groupsHandler.TryDeleteGroup(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }


    [Route("get_participant_calendar")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetGroupParticipantCalendarInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _groupsHandler.GetGroupParticipantCalendar(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    private readonly IGroupsHandler _groupsHandler;
}
