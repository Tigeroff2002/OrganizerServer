using Contracts.Request;
using Contracts.Response;
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

namespace OrganizerServer.Controllers;

[ApiController]
[Route("alerts")]
public sealed class AlertController : ControllerBase
{
    public AlertController(
        IAlertsRepository alertsRepository,
        IUsersRepository usersRepository)
    {
        _alertsRepository = alertsRepository
            ?? throw new ArgumentNullException(nameof(alertsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));
    }

    [Route("get_all_alerts")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetAllIssues(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var requestDTO = JsonConvert.DeserializeObject<RequestWithToken>(body);

        Debug.Assert(requestDTO != null);

        var userId = requestDTO.UserId;

        var existedUser = await _usersRepository.GetUserByIdAsync(userId, token);

        if (existedUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Cant take info about system alerts cause " +
                $"user with id {userId} is not found in db";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        if (existedUser.Role != UserRole.Admin)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Cant take info about system alerts cause " +
                $"user with id {userId} is not system administrator";

            return Forbid(JsonConvert.SerializeObject(response1));
        }

        var allIAlerts =
            _alertsRepository
                .GetAllAlertsAsync(token)
                .GetAwaiter().GetResult()
                .Select(x => new AlertInfoResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    IsAlerted = x.IsAlerted,
                    Moment = x.Moment
                })
                .ToList();

        var systemAlertsResponseModel = new SystemAlertsResponse
        {
            Alerts = allIAlerts
        };

        var getResponse = new GetResponse();
        getResponse.Result = true;
        getResponse.OutInfo =
            $"Info about all system alerts" +
            $" for admin with id {userId} was received";
        getResponse.RequestedInfo = systemAlertsResponseModel;

        var json = JsonConvert.SerializeObject(getResponse);

        return Ok(json);
    }

    private readonly IAlertsRepository _alertsRepository;
    private readonly IUsersRepository _usersRepository;
}
