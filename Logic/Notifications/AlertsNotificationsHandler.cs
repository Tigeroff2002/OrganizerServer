using Logic.Abstractions;
using Logic.Transport.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.Enums;
using Models.StorageModels;
using Models.UserActionModels.NotificationModels;
using PostgreSQL.Abstractions;

namespace Logic.Notifications;

public sealed class AlertsNotificationsHandler
    : IAlertsNotificationsHandler
{
    public AlertsNotificationsHandler(
        IPushNotificationsSender pushNotificationsSender,
        ICommonUsersUnitOfWork usersUnitOfWork,
        IOptions<NotificationConfiguration> notifyConfiguration,
        ILogger<EventNotificationsHandler> logger)
    {
        _pushNotificationsSender = pushNotificationsSender
            ?? throw new ArgumentNullException(nameof(pushNotificationsSender));

        _usersUnitOfWork = usersUnitOfWork
            ?? throw new ArgumentNullException(nameof(usersUnitOfWork));

        ArgumentNullException.ThrowIfNull(notifyConfiguration);

        _notifyConfiguration = notifyConfiguration.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAlersAsync(CancellationToken token)
    {
        _logger.LogInformation("Starting handling alerts in target handler");

        while (!token.IsCancellationRequested)
        {
            var dbAlerts = await _usersUnitOfWork.AlertsRepository.GetAllAlertsAsync(token);

            await HandleAlertsNotificationsAsync(dbAlerts, token);

            _usersUnitOfWork.SaveChanges();

            Task.Delay(_notifyConfiguration.IterationDelay, token).GetAwaiter().GetResult();
        }
    }

    private async Task HandleAlertsNotificationsAsync(
        List<Alert>? dbAlerts,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (dbAlerts == null)
        {
            return;
        }

        var currentTiming = DateTimeOffset.Now;

        foreach (var dbAlert in dbAlerts)
        {
            var isNotificationRequired =
                !dbAlert.IsAlerted
                && currentTiming.Subtract(dbAlert.Moment).Days < _notifyConfiguration.FutureStockDays;

            if (isNotificationRequired)
            {
                await SendNotificationsForUsersAsync(dbAlert.Description, token);
            }

            await _usersUnitOfWork.AlertsRepository.MarkAsAlertedAsync(dbAlert.Id, token);
        }
    }

    private async Task SendNotificationsForUsersAsync(
        string content,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var users = await
            _usersUnitOfWork.UsersRepository
                .GetAllUsersAsync(token);

        var admins = users.Where(x => x.Role == UserRole.Admin).ToList();

        if (!admins.Any())
        {
            _logger.LogInformation("Alert message was not sent to anyone");
        }

        var allDevicesMaps =
            await _usersUnitOfWork.UserDevicesRepository
                .GetAllDevicesMapsAsync(token);

        foreach (var admin in admins)
        {
            var userName = admin.UserName;

            var subject = "Alerting about server error incident";

            var adsPushReminderMessages =
                CreateAdsPushContentMessages(
                    admin.Id,
                    subject,
                    userName,
                    content,
                    allDevicesMaps);

            await Task.WhenAll(
                adsPushReminderMessages.Select(
                    ads => Task.Run(
                        async () => await _pushNotificationsSender
                            .SendAdsPushNotificationAsync(ads, token))));

            _logger.LogInformation(
                "Reminders of ads push for user" +
                " with name {UserName} has been succesfully sent",
                userName);
        }
    }

    private List<UserAdsPushContentInfo> CreateAdsPushContentMessages(
        int userId,
        string subject,
        string userName,
        string content,
        List<UserDeviceMap> allDeviceMaps)
    {
        var userMaps =
            allDeviceMaps
                .Where(x => x.UserId == userId && x.IsActive)
                .ToList();

        return userMaps
            .Select(x =>
                new UserAdsPushContentInfo(
                    subject,
                    userName,
                    content,
                    x.FirebaseToken))
            .ToList();
    }

    private readonly IPushNotificationsSender _pushNotificationsSender;
    private readonly ICommonUsersUnitOfWork _usersUnitOfWork;
    private readonly NotificationConfiguration _notifyConfiguration;
    private readonly ILogger<EventNotificationsHandler> _logger;
}
