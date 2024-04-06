using AdsPush;
using AdsPush.Abstraction;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.UserActionModels;
using System.Text;

namespace Logic.Transport.Senders;

public sealed class PushNotificationsSender
    : IPushNotificationsSender
{
    public PushNotificationsSender(
        IAdsPushSenderFactory adsPushSenderFactory,
        IOptions<AdsPushConfiguration> options,
        ILogger<PushNotificationsSender> logger)
    {
        ArgumentNullException.ThrowIfNull(adsPushSenderFactory);
        ArgumentNullException.ThrowIfNull(options);

        _adsPushSender = adsPushSenderFactory.GetSender("MyApp");
        _adsPushConfiguration = options.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task SendAdsPushNotificationAsync(
        UserNotificationInfo model,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(model);

        token.ThrowIfCancellationRequested();

        var body = new StringBuilder();

        if (model is UserAdsPushReminderInfo reminderModel)
        {
            var numberMinutesOfOffset = reminderModel.TotalMinutes;

            body.Append($"Hello, {model.UserName}!\n");
            body.Append(
                $"We are sending you a reminder that your event" +
                $" will start in less than {numberMinutesOfOffset} minutes.\n");

            body.Append(reminderModel.EventName);

            await SendNotificationAsync(
                body.ToString(), reminderModel, reminderModel.FirebaseToken, token);
        }
        else if (model is UserAdsPushContentInfo contentModel)
        {
            body.Append($"Hello, {model.UserName}!\n");
            body.Append(
                $"We are sending you alert message, that you can see below: \n");

            body.Append(contentModel.Content);

            await SendNotificationAsync(
                body.ToString(), contentModel, contentModel.FirebaseToken, token);
        }
        else
        {
            throw new InvalidOperationException(
                $"Model type: {model.GetType().Name} is not supported" +
                $" for sending push notifications messages.");
        }
    }

    private async Task SendNotificationAsync(
        string body,
        UserNotificationInfo model,
        string firebaseToken,
        CancellationToken token)
    {
        var message = new Message()
        {
            Token = firebaseToken,
            Android = new AndroidConfig()
            {
                Priority = Priority.High,
            },
            Notification = new()
            {
                Title = model.Subject,
                Body = body.ToString()
            }
        };

        try
        {
            var firebaseResult =
                await _adsPushSender
                    .GetFirebaseSender()
                    .SendToSingleAsync(message, token);

            if (firebaseResult != null)
            {
                if (firebaseResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "New notification message with id {MessageId}" +
                        " was sended succesfully",
                        firebaseResult.MessageId);
                }
            }
        }

        catch (Exception ex)
        {
            _logger.LogWarning("Exception occured: {Message}", ex.Message);
        }
    }

    private readonly IAdsPushSender _adsPushSender;
    private readonly AdsPushConfiguration _adsPushConfiguration;
    private readonly ILogger<PushNotificationsSender> _logger;
}
