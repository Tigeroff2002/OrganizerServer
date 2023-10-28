using Contracts.Response;
using Logic.Abstractions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Models;
using Models.Enums;
using Org.BouncyCastle.Asn1.Ess;
using PostgreSQL.Abstractions;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Logic;

public sealed class EventNotificationsHandler
    : IEventNotificationsHandler
{
    public EventNotificationsHandler(
        IUsersRepository usersRepository,
        IEventsRepository eventsRepository,
        IEventsUsersMapRepository eventsUsersMapRepository,
        IOptions<SmtpConfiguration> smtpConfiguration,
        IOptions<NotificationConfiguration> notifyConfiguration,
        ILogger<EventNotificationsHandler> logger) 
    { 
        _usersRepository = usersRepository 
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _eventsRepository = eventsRepository 
            ?? throw new ArgumentNullException(nameof(eventsRepository));

        _eventsUsersMapRepository = eventsUsersMapRepository
            ?? throw new ArgumentNullException(nameof(eventsUsersMapRepository));

        ArgumentNullException.ThrowIfNull(smtpConfiguration);
        ArgumentNullException.ThrowIfNull(notifyConfiguration);

        _smtpConfiguration = smtpConfiguration.Value;
        _notifyConfiguration = notifyConfiguration.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventsAsync(CancellationToken token)
    {
        _logger.LogInformation("Starting handling events in target handler");
        
        while(!token.IsCancellationRequested)
        {
            await HandleEventStatusesASync(token);

            await HandleEventsNotificationsAsync(token);

            Task.Delay(_notifyConfiguration.IterationDelay).GetAwaiter().GetResult();
        }
    }

    private async Task HandleEventStatusesASync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var dbEvents = await _eventsRepository.GetAllEventsAsync(token);

        var currentTiming = DateTimeOffset.Now;

        foreach(var dbEvent in dbEvents)
        {
            if (dbEvent.ScheduledStart <= currentTiming) 
            {
                if (dbEvent.Status != EventStatus.Cancelled)
                {
                    var oldStatus = dbEvent.Status;

                    dbEvent.Status = EventStatus.Live;

                    var endEvent = dbEvent.ScheduledStart.Add(dbEvent.Duration);

                    if (currentTiming >= endEvent)
                    {
                        dbEvent.Status = EventStatus.Finished;
                    }

                    _logger.LogDebug(
                        "Status of event {EventId} was changed" +
                        " from {OldStatus} to {NewStatus}",
                        dbEvent.Id,
                        oldStatus,
                        dbEvent.Status);
                }
            }
        }
    }

    private async Task HandleEventsNotificationsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var dbEvents = await _eventsRepository.GetAllEventsAsync(token);

        var currentTiming = DateTimeOffset.Now;

        var timingWithOffset = currentTiming.Add(_notifyConfiguration.ReminderOffset);

        foreach (var dbEvent in dbEvents)
        {
            if (dbEvent.ScheduledStart <= timingWithOffset)
            {
                if (dbEvent.Status != EventStatus.Cancelled)
                {
                    if (dbEvent.Status == EventStatus.NotStarted)
                    {
                        dbEvent.Status = EventStatus.WithinReminderOffset;

                        _logger.LogDebug(
                            "Status of event {EventId} was changed" +
                            " from {OldStatus} to {NewStatus}",
                            dbEvent.Id,
                            EventStatus.NotStarted,
                            EventStatus.WithinReminderOffset);

                        _logger.LogDebug(
                            "Start reminding users related for visiting event {EventId}",
                            dbEvent.Id);

                        var eventName = new StringBuilder();
                        var endOfEvent = dbEvent.ScheduledStart.Add(dbEvent.Duration);

                        eventName.Append($"Event info:\n");
                        eventName.Append("Caption: {dbEvent.Caption}\n");
                        eventName.Append($"Description: {dbEvent.Description}\n");
                        eventName.Append($"Event timing borders: {dbEvent.ScheduledStart} : {endOfEvent}.");

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

                _logger.LogDebug(
                    "Sending reminder for user {UserName} to email adress {Email}",
                    userName,
                    email);

                await SendMessageFromSmtpAsync(eventName, userName, email, token);

                _logger.LogDebug("Reminder has been succesfully sent");
            }
        }
    }

    private async Task SendMessageFromSmtpAsync(
        string eventName,
        string userName,
        string email,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var smtpClient = new MailKit.Net.Smtp.SmtpClient();

        await smtpClient.ConnectAsync(
            _smtpConfiguration.Host,
            _smtpConfiguration.Port,
            useSsl: true,
            token);

        await smtpClient.AuthenticateAsync(
            _smtpConfiguration.FromAdress,
            _smtpConfiguration.FromPassword);

        _logger.LogInformation("Connected to smtp server");

        var subject = "Reminder for the near beginning of your event";

        var body = new StringBuilder();
        var numberMinutesOfOffset = _notifyConfiguration.ReminderOffset.TotalMinutes;

        body.Append($"Hello, {userName}!\n");
        body.Append(
            $"We are sending you a reminder that your event" +
            $" will start in less than {numberMinutesOfOffset} minutes.\n");
        body.Append(eventName);

        var mailMessage = new MailMessage(
            new MailAddress(_smtpConfiguration.FromAdress),
            new MailAddress(email))
        {
            Subject = subject,
            Body = body.ToString()
        };

        var response = await smtpClient.SendAsync(
            MimeMessage.CreateFromMailMessage(mailMessage), token);
    }

    private readonly IUsersRepository _usersRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly IEventsUsersMapRepository _eventsUsersMapRepository;
    private readonly SmtpConfiguration _smtpConfiguration;
    private readonly NotificationConfiguration _notifyConfiguration;
    private readonly ILogger<EventNotificationsHandler> _logger;
}
