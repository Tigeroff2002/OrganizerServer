using AdsPush.Abstraction.Firebase;
using AdsPush.Abstraction.Settings;
using AdsPush.Abstraction;
using AdsPush.Firebase;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Logic.Transport.Senders.Models;

namespace Logic.Transport.Senders;

public sealed class FirebasePushNotificationSender
{
    private readonly FirebaseMessaging _firebaseMessaging;

    public FirebasePushNotificationSender(AdsPushFirebaseSettings settings)
    {
        var serviceAccountModel = new ServiceAccount()
        {
            Type = settings.Type,
            AuthUri = settings.AuthUri,
            ClientEmail = settings.ClientEmail,
            ClientId = settings.ClientId,
            PrivateKeyId = settings.PrivateKeyId,
            PrivateKey = settings.PrivateKey,
            ProjectId = settings.ProjectId,
            TokenUri = settings.TokenUri,
            ClientX509CertUrl = settings.ClientX509CertUrl,
            AuthProviderX509CertUrl = settings.AuthProviderX509CertUrl
        };

        var serviceAccountJson = JsonConvert.SerializeObject(serviceAccountModel);
        var firebaseApp = FirebaseApp.GetInstance(settings.ProjectId) 
            ?? FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromJson(serviceAccountJson) }, settings.ProjectId);

        this._firebaseMessaging = FirebaseMessaging.GetMessaging(firebaseApp);
    }

    public FirebasePushNotificationSender(string serviceAccountJson)
    {
        var firebaseApp = FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromJson(serviceAccountJson)
        });

        this._firebaseMessaging = FirebaseMessaging.GetMessaging(firebaseApp);
    }

    public FirebasePushNotificationSender(Stream stream)
    {
        var firebaseApp = FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromStream(stream)
        });

        this._firebaseMessaging = FirebaseMessaging.GetMessaging(firebaseApp);
    }

    /// <inheritdoc />
    public async Task<FirebaseNotificationResult> SendToSingleAsync(
        Message notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messageId = await this._firebaseMessaging.SendAsync(notification, cancellationToken);
            return FirebaseNotificationResult.CreateSuccessResult(messageId);
        }
        catch (FirebaseMessagingException e)
        {
            return FirebaseNotificationResult.CreateFailedResult(e);
        }
    }

    /// <inheritdoc />
    public async Task<FirebaseNotificationBatchResult> SendToMultiDeviceAsync(
        MulticastMessage notification,
        CancellationToken cancellationToken = default)
    {
        var result = await this._firebaseMessaging.SendMulticastAsync(notification, cancellationToken);
        return new FirebaseNotificationBatchResult(result.Responses.Select(FirebaseNotificationResult.CreateUsingFirebaseSendResponse).ToList());
    }

    /// <inheritdoc />
    public async Task<FirebaseNotificationBatchResult> SendBatchNotificationAsync(
        IEnumerable<Message> notifications,
        CancellationToken cancellationToken = default)
    {
        var result = await this._firebaseMessaging.SendAllAsync(notifications, cancellationToken);
        return new FirebaseNotificationBatchResult(result.Responses.Select(FirebaseNotificationResult.CreateUsingFirebaseSendResponse).ToList());
    }

    public async Task SendAsync(
        AdsPushTarget target,
        string token,
        AdsPushBasicSendPayload payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = payload.CreateFirebaseMessage(target, token);
            await this._firebaseMessaging.SendAsync(message, cancellationToken);
        }
        catch (FirebaseException e)
        {
            throw e.CreateException();
        }
    }
}
