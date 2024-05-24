using AdsPush;
using AdsPush.Abstraction;
using AdsPush.Abstraction.Settings;
using AdsPush.Firebase;
using AdsPush.Firebase.Settings;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.UserActionModels.NotificationModels;
using System.Reflection;
using System.Text;

namespace Logic.Transport.Senders;

public sealed class PushNotificationsSender
    : IPushNotificationsSender
{
    public PushNotificationsSender(
        IOptions<AdsPushFirebaseSettingsConfiguration> options,
        ILogger<PushNotificationsSender> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _configuration = options.Value;

        var settings = new AdsPushFirebaseSettings
        {
            Type = _configuration.Type,
            ProjectId = _configuration.ProjectId,
            PrivateKey = _configuration.PrivateKey,
            PrivateKeyId = _configuration.PrivateKeyId,
            ClientId = _configuration.ClientId,
            ClientEmail = _configuration.ClientEmail,
            AuthUri = _configuration.AuthUri,
            AuthProviderX509CertUrl = _configuration.AuthProviderX509CertUrl,
            TokenUri = _configuration.TokenUri,
            ClientX509CertUrl = _configuration.ClientX509CertUrl
        };

        _adsPushSender = new FirebasePushNotificationSender(settings);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    

    public async Task SendAdsPushNotificationAsync(
        UserAdsPushContentInfo model,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(model);

        token.ThrowIfCancellationRequested();

        var body = new StringBuilder();

        body.Append($"Hello, {model.UserName}!\n");
        body.Append(
            $"We are sending you reminder message, that you can see below: \n");

        body.Append(model.Description);

        await SendNotificationAsync(
            body.ToString(), model, model.FirebaseToken, token);
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
                    .SendToSingleAsync(message, token);

            _logger.LogInformation(
                "Trying to send new notification message to user {UserName}",
                model.UserName);

            if (firebaseResult != null)
            {
                if (firebaseResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "New notification message with id {MessageId}" +
                        " was sended succesfully for user {UserName}",
                        firebaseResult.MessageId,
                        model.UserName);
                }
            }
        }

        catch (Exception ex)
        {
            _logger.LogWarning("Exception occured: {Message}", ex.Message);
        }
    }

    private readonly FirebasePushNotificationSender _adsPushSender;
    private readonly AdsPushFirebaseSettingsConfiguration _configuration;
    private readonly ILogger<PushNotificationsSender> _logger;
}
