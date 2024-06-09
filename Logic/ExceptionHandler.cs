using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using Models.BusinessModels;
using Models.Enums;
using Models.RedisEventModels.AlertEvents;
using Models.StorageModels;
using PostgreSQL.Abstractions;
using System.Text.Json;

namespace Logic;

public sealed class ExceptionHandler : IExceptionHandler
{
    public ExceptionHandler(
        ICommonUsersUnitOfWork usersUnitOfWork,
        IRedisRepository redisRepository,
        ILogger<ExceptionHandler> logger)
    {
        _userUnitOfWork = usersUnitOfWork 
            ?? throw new ArgumentNullException(nameof(usersUnitOfWork));

        _redisRepository = redisRepository 
            ?? throw new ArgumentNullException(nameof(redisRepository));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> HandleExceptionsAsync(Exception exception, CancellationToken token)
    {
        var errorResponse = "Server error occured. ";

        errorResponse += $"Exception: {exception.Message}";

        var (alertId, alertMoment) =
            await CreateBagAlert(errorResponse, CancellationToken.None);

        _logger.LogError(errorResponse);

        var resultJson = JsonSerializer.Serialize(errorResponse);

        var admins =
            _userUnitOfWork
                .UsersRepository
                .GetAllUsersAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult()
                .Where(x => x.Role == UserRole.Admin);

        foreach (var user in admins)
        {
            var @event = new AlertCreatedEvent(
                    Id: Guid.NewGuid().ToString(),
                    IsCommited: false,
                    UserId: user.Id,
                    AlertId: alertId,
                    CreateMoment: alertMoment,
                    Json: resultJson);

            _logger.LogDebug("Preparing to send event {EventId} to cache", @event.Id);

            await _redisRepository.InsertEventAsync(@event);
        }

        return await Task.FromResult(resultJson!);
    }

    private async Task<(int, DateTimeOffset)> CreateBagAlert(
        string message, 
        CancellationToken token)
    {
        var alertMoment = DateTimeOffset.UtcNow;

        var title = "Server internal error 500";

        var alert = new Alert
        {
            Title = title,
            Description = message!,
            Moment = alertMoment,
            IsAlerted = false
        };

        await _userUnitOfWork
            .AlertsRepository
            .AddAsync(alert, token);

        _userUnitOfWork.SaveChanges();

        return await Task.FromResult((alert.Id, alertMoment));
    }

    private readonly ILogger _logger;
    private readonly ICommonUsersUnitOfWork _userUnitOfWork;
    private readonly IRedisRepository _redisRepository;
}
