using Logic.Abstractions;
using Logic.Transport.Senders;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Models;
using Models.Enums;
using Models.UserActionModels;
using PostgreSQL.Abstractions;
using System.Net.Mail;
using System.Text;

namespace Logic;

public sealed class EventNotificationsHandler
    : IEventNotificationsHandler
{
    public EventNotificationsHandler(
        ISMTPSender smtpSender,
        IPushNotificationsSender pushNotificationsSender,
        IUsersUnitOfWork usersUnitOfWork,
        IOptions<NotificationConfiguration> notifyConfiguration,
        ILogger<EventNotificationsHandler> logger) 
    { 
        _smtpSender = smtpSender
            ?? throw new ArgumentNullException(nameof(smtpSender));

        _pushNotificationsSender = pushNotificationsSender
            ?? throw new ArgumentNullException(nameof(pushNotificationsSender));

        _usersUnitOfWork = usersUnitOfWork
            ?? throw new ArgumentNullException(nameof(usersUnitOfWork));

        ArgumentNullException.ThrowIfNull(notifyConfiguration);

        _notifyConfiguration = notifyConfiguration.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventsAsync(CancellationToken token)
    {
        _logger.LogInformation("Starting handling events in target handler");
        
        while (!token.IsCancellationRequested)
        {
            var dbEvents = await _usersUnitOfWork.EventsRepository.GetAllEventsAsync(token);

            await HandleEventsNotificationsAsync(dbEvents, token);

            HandleEventStatuses(dbEvents, token);

            _usersUnitOfWork.SaveChanges();

            Task.Delay(_notifyConfiguration.IterationDelay, token).GetAwaiter().GetResult();
        }
    }

    private void HandleEventStatuses(
        List<Event>? dbEvents, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var currentTiming = DateTimeOffset.Now;

        if (dbEvents == null)
        {
            return;
        }

        foreach (var dbEvent in dbEvents)
        {
            if (dbEvent.ScheduledStart <= currentTiming) 
            {
                if (dbEvent.Status != EventStatus.Cancelled 
                    && dbEvent.Status != EventStatus.Finished)
                {
                    var oldStatus = dbEvent.Status;

                    if (dbEvent.Status is EventStatus.NotStarted 
                        or EventStatus.WithinReminderOffset)
                    {
                        dbEvent.Status = EventStatus.Live;
                    }

                    var endEvent = dbEvent.ScheduledStart.Add(dbEvent.Duration);

                    if (currentTiming >= endEvent)
                    {
                        if (dbEvent.Status != EventStatus.Finished)
                        {
                            dbEvent.Status = EventStatus.Finished;
                        }
                    }

                    _logger.LogInformation(
                        "Status of event {EventId} was changed" +
                        " from {OldStatus} to {NewStatus}",
                        dbEvent.Id,
                        oldStatus,
                        dbEvent.Status);
                }
            }
        }
    }

    private async Task HandleEventsNotificationsAsync(
        List<Event>? dbEvents,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (dbEvents == null)
        {
            return;
        }

        var currentTiming = DateTimeOffset.Now;

        var timingWithOffset = currentTiming.Add(_notifyConfiguration.ReminderOffset);

        foreach (var dbEvent in dbEvents)
        {
            var isNotificationRequired = 
                dbEvent.ScheduledStart <= timingWithOffset 
                && dbEvent.ScheduledStart > currentTiming;

            if (isNotificationRequired)
            {
                if (dbEvent.Status != EventStatus.Cancelled)
                {
                    if (dbEvent.Status == EventStatus.NotStarted
                        && dbEvent.Status != EventStatus.Finished)
                    {
                        dbEvent.Status = EventStatus.WithinReminderOffset;

                        _logger.LogInformation(
                            "Status of event {EventId} was changed" +
                            " from {OldStatus} to {NewStatus}",
                            dbEvent.Id,
                            EventStatus.NotStarted,
                            EventStatus.WithinReminderOffset);

                        _logger.LogInformation(
                            "Start reminding users related for visiting event {EventId}",
                            dbEvent.Id);

                        var eventName = new StringBuilder();
                        var endOfEvent = dbEvent.ScheduledStart.Add(dbEvent.Duration);

                        eventName.Append($"Event info:\n");
                        eventName.Append($"Caption: {dbEvent.Caption}\n");
                        eventName.Append($"Description: {dbEvent.Description}\n");
                        eventName.Append(
                            $"Event timing borders: {dbEvent.ScheduledStart.ToLocalTime()}" +
                            $" : {endOfEvent.ToLocalTime()}.");

                        // отправка уведомлений по SMTP и ADS PUSH
                        await SendRemindersForUsersAsync(dbEvent.Id, eventName.ToString(), token);
                    }
                }
            }
        }
    }

    private async Task SendRemindersForUsersAsync(
        int eventId, string eventName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var guestsMaps = await 
            _usersUnitOfWork.EventsUsersMapRepository
                .GetAllMapsAsync(token);

        var guestsMapsRelatedToEvent = 
            guestsMaps
                .Where(map => map.EventId == eventId)
                .ToList();

        var users = await 
            _usersUnitOfWork.UsersRepository
                .GetAllUsersAsync(token);

        var allDevicesMaps = 
            await _usersUnitOfWork.UserDevicesRepository
                .GetAllDevicesMapsAsync(token);

        foreach (var map in guestsMapsRelatedToEvent)
        {
            var userId = map.UserId;

            var currentUser = users.FirstOrDefault(x => x.Id == userId);

            if (currentUser != null)
            {
                var userName = currentUser.UserName;
                var email = currentUser.Email;

                _logger.LogInformation(
                    "Sending reminder for user {UserName} to email adress {Email}",
                    userName,
                    email);

                var subject = "Reminder for the near beginning of your event";

                var totalMinutes = (int)_notifyConfiguration.ReminderOffset.TotalMinutes;

                var reminderSMTPInfo = 
                    new UserEmailReminderInfo(
                        subject,
                        eventName,
                        userName,
                        email,
                        totalMinutes);

                var adsPushReminderMessages =
                    CreateAdsPushReminderMessages(
                        userId,
                        subject,
                        eventName, 
                        userName,
                        totalMinutes,
                        allDevicesMaps);

                await _smtpSender.SendSMTPNotificationAsync(reminderSMTPInfo, token);

                _logger.LogInformation(
                    "Reminder for SMTP for user" +
                    " with email {Email} has been succesfully sent",
                    email);

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
    }

    private List<UserAdsPushReminderInfo> CreateAdsPushReminderMessages(
        int userId,
        string subject,
        string eventName,
        string userName,
        int totalMinutes,
        List<UserDeviceMap> allDeviceMaps)
    {
        var userMaps = 
            allDeviceMaps
                .Where(x => x.UserId == userId && x.IsActive)
                .ToList();

        return userMaps
            .Select(x => 
                new UserAdsPushReminderInfo(
                    subject, 
                    eventName,
                    userName,
                    x.FirebaseToken,
                    totalMinutes))
            .ToList();
    }

    private readonly ISMTPSender _smtpSender;
    private readonly IPushNotificationsSender _pushNotificationsSender;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUsersUnitOfWork _usersUnitOfWork;
    private readonly NotificationConfiguration _notifyConfiguration;
    private readonly ILogger<EventNotificationsHandler> _logger;
}
