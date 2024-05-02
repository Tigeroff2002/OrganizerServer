using Contracts;
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
[Route("events")]
public sealed class EventController : ControllerBase
{
    public EventController(
        IEventsHandler eventsHandler)
    {
        _eventsHandler = eventsHandler 
            ?? throw new ArgumentNullException(nameof(eventsHandler));
    }

    [HttpPost]
    [Route("schedule_new")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> ScheduleNewEvent(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _eventsHandler.TryScheduleEvent(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("update_event_params")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateEventParams(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _eventsHandler.TryUpdateEvent(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("change_user_decision_for_event")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> ChangeUserDecisionForEvent(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _eventsHandler.TryDeleteEvent(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("delete_event")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteEvent(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _eventsHandler.TryDeleteEvent(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("get_event_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetEventInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _eventsHandler.GetEventInfo(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    private readonly IEventsHandler _eventsHandler;
}

