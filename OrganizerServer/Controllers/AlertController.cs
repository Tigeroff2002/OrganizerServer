using Contracts.Request;
using Contracts.Response;
using Logic.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.BusinessModels;
using Models.Enums;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using ToDoCalendarServer;
using ToDoCalendarServer.Controllers;
using static Google.Apis.Requests.BatchRequest;

namespace OrganizerServer.Controllers;

[ApiController]
[Route("alerts")]
public sealed class AlertController : ControllerBase
{
    public AlertController(
        IAlertsReceiverHandler alertsReceiverHandler)
    {
        _alertsReceiverHandler = alertsReceiverHandler
            ?? throw new ArgumentNullException(nameof(alertsReceiverHandler));
    }

    [Route("get_all_alerts")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetAllIssues(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _alertsReceiverHandler.GetAllAlerts(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    private readonly IAlertsReceiverHandler _alertsReceiverHandler;
}
