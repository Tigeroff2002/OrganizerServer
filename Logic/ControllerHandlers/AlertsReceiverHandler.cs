using Contracts.Request;
using Contracts.Response;
using Logic.Abstractions;
using Models.BusinessModels;
using Models.Enums;
using Newtonsoft.Json;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace Logic.ControllerHandlers;

public sealed class AlertsReceiverHandler
    : IAlertsReceiverHandler
{
    public AlertsReceiverHandler(ICommonUsersUnitOfWork commonUnitOfWork)
    {
        _commonUnitOfWork = commonUnitOfWork
            ?? throw new ArgumentNullException(nameof(commonUnitOfWork));
    }

    public async Task<GetResponse> GetAllAlerts(
        string allAlertsDTO,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(allAlertsDTO);

        token.ThrowIfCancellationRequested();

        var requestDTO = 
            JsonConvert.DeserializeObject<RequestWithToken>(allAlertsDTO);

        Debug.Assert(requestDTO != null);

        var userId = requestDTO.UserId;

        var existedUser = await _commonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(userId, token);

        if (existedUser == null)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo =
                $"Cant take info about system alerts cause " +
                $"user with id {userId} is not found in db";

            return await Task.FromResult(response1);
        }

        if (existedUser.Role != UserRole.Admin)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo =
                $"Cant take info about system alerts cause " +
                $"user with id {userId} is not system administrator";

            return await Task.FromResult(response1);
        }

        var allIAlerts =
            _commonUnitOfWork
                .AlertsRepository
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

        return await Task.FromResult(getResponse);
    }

    private readonly ICommonUsersUnitOfWork _commonUnitOfWork;
}
