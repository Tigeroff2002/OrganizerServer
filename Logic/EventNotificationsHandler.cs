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
        IUsersRepository usersRepository,
        IServiceProvider serviceProvider,
        IEventsUsersMapRepository eventsUsersMapRepository,
        IOptions<NotificationConfiguration> notifyConfiguration,
        ILogger<EventNotificationsHandler> logger) 
    { 
        _smtpSender = smtpSender
            ?? throw new ArgumentNullException(nameof(smtpSender));

        _usersRepository = usersRepository 
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _serviceProvider = serviceProvider 
            ?? throw new ArgumentNullException(nameof(serviceProvider));

        _eventsUsersMapRepository = eventsUsersMapRepository
            ?? throw new ArgumentNullException(nameof(eventsUsersMapRepository));

        ArgumentNullException.ThrowIfNull(notifyConfiguration);

        _notifyConfiguration = notifyConfiguration.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventsAsync(CancellationToken token)
    {
        _logger.LogInformation("Starting handling events in target handler");
        
        while (!token.IsCancellationRequested)
        {
            using var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();

            var dbEvents = await eventRepository.GetAllEventsAsync(token);

            await HandleEventsNotificationsAsync(dbEvents, token);

            HandleEventStatuses(dbEvents, token);

            eventRepository.SaveChanges();

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

        var guestsMaps = await _eventsUsersMapRepository
            .GetAllMapsAsync(token);

        var guestsMapsRelatedToEvent = guestsMaps
            .Where(map => map.EventId == eventId)
            .ToList();

        var users = await _usersRepository.GetAllUsersAsync(token);

        foreach(var map in guestsMapsRelatedToEvent)
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

                var reminderInfoModel = new UserReminderInfo(
                    subject,
                    eventName,
                    userName,
                    email,
                    (int)_notifyConfiguration.ReminderOffset.TotalMinutes);

                await _smtpSender.SendNotificationAsync(reminderInfoModel, token);

                _logger.LogInformation("Reminder has been succesfully sent");
            }
        }
    }

    private readonly ISMTPSender _smtpSender;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUsersRepository _usersRepository;
    private readonly IEventsUsersMapRepository _eventsUsersMapRepository;
    private readonly NotificationConfiguration _notifyConfiguration;
    private readonly ILogger<EventNotificationsHandler> _logger;
}
