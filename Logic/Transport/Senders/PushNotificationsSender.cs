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


    public async Task SendNotificationAsync(UserReminderInfo model, CancellationToken token)
    {
        var body = new StringBuilder();

        var numberMinutesOfOffset = model.TotalMinutes;

        body.Append($"Hello, {model.UserName}!\n");
        body.Append(
            $"We are sending you a reminder that your event" +
            $" will start in less than {numberMinutesOfOffset} minutes.\n");

        body.Append(model.EventName);

        var message = new Message()
        {
            Token = PUSH_TOKEN,
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
                    .SendToSingleAsync(message);

            if (firebaseResult != null)
            {
                if (firebaseResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "New notification message with id {MessageId} was sended succesfully",
                        firebaseResult.MessageId);
                }
            }
        }

        catch (Exception ex)
        {
            _logger.LogWarning("Exception occured: {Message}", ex.Message);
        }
    }

    private const string PUSH_TOKEN = "79eb1b9e623bbca0d2b218f44a18d7b8ef59dac4da5baa9949c3e99a48eb259a";

    private readonly IAdsPushSender _adsPushSender;
    private readonly AdsPushConfiguration _adsPushConfiguration;
    private readonly ILogger<PushNotificationsSender> _logger;
}
