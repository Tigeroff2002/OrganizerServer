using Logic.Abstractions;
using Logic.ControllerHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models.BusinessModels;
using Models.Enums;
using Models.RedisEventModels.AlertEvents;
using Models.StorageModels;
using PostgreSQL.Abstractions;
using System.Net;
using System.Text.Json;

namespace Logic;

public sealed class CustomExceptionsHandler : DataHandlerBase
{
    public CustomExceptionsHandler(
        RequestDelegate next,
        ILogger<CustomExceptionsHandler> logger,
        ICommonUsersUnitOfWork commomUnitOfWork,
        IRedisRepository redisRepository)
        : base(commomUnitOfWork, redisRepository, logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = new Response
        {
            Result = false,
            OutInfo = "Server error occured. "
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        errorResponse.OutInfo += $"Exception: {exception.Message}";

        var (alertId, alertMoment) = 
            await CreateBagAlert(errorResponse, CancellationToken.None);

        Logger.LogError(errorResponse.OutInfo);

        var resultJson = JsonSerializer.Serialize(errorResponse);

        var admins =
            CommonUnitOfWork
                .UsersRepository
                .GetAllUsersAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult()
                .Where(x => x.Role == UserRole.Admin);

        foreach (var user in admins)
        {
            await SendEventForCacheAsync(
                new AlertCreatedEvent(
                    Id: Guid.NewGuid().ToString(),
                    IsCommited: false,
                    UserId: user.Id,
                    AlertId: alertId,
                    CreateMoment: alertMoment,
                    Json: resultJson));
        }

        await context.Response.WriteAsync(resultJson);
    }

    private async Task<(int, DateTimeOffset)> CreateBagAlert(Response message, CancellationToken token)
    {
        var alertMoment = DateTimeOffset.UtcNow;

        var title = "Server internal error 500";

        var alert = new Alert
        {
            Title = title,
            Description = message.OutInfo!,
            Moment = alertMoment,
            IsAlerted = false
        };

        await CommonUnitOfWork
            .AlertsRepository
            .AddAsync(alert, token);

        CommonUnitOfWork.SaveChanges();

        return await Task.FromResult((alert.Id, alertMoment));
    }

    private readonly RequestDelegate _next;
}