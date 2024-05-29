using Logic.Abstractions;
using Logic.Transport.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.StorageModels;
using Models.UserActionModels;
using Models.UserActionModels.NotificationModels;
using PostgreSQL.Abstractions;

namespace Logic;

public sealed class RedisProcessor : IRedisProcessor
{
    public RedisProcessor(
        ILogger<RedisProcessor> logger, 
        IRedisMessagesReceiver receiver, 
        ISMTPSender smtpSender,
        IPushNotificationsSender adsSender,
        ICommonUsersUnitOfWork commonUnitOfWork,
        IRedisEventsAliaser aliaser,
        IOptions<RedisReadingConfiguration> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _configuration = options.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _smtpSender = smtpSender ?? throw new ArgumentNullException(nameof(smtpSender));

        _adsSender = adsSender ?? throw new ArgumentNullException(nameof(adsSender));

        _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));

        _commonUnitOfWork = commonUnitOfWork
            ?? throw new ArgumentNullException(nameof(commonUnitOfWork));

        _eventsAliaser = aliaser ?? throw new ArgumentNullException(nameof(aliaser));
    }

    public async Task ProcessAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var allUsers = await _commonUnitOfWork.UsersRepository.GetAllUsersAsync(token);

            _logger.LogInformation("Beginning next iteration of processing redis messages");

            var messages = await _receiver.GetMessages(token);

            foreach (var message in messages)
            {
                var userId = message.UserId;

                var user = allUsers.FirstOrDefault(x => x.Id == userId);

                if (user == null)
                {
                    continue;
                }

                var eventName = _eventsAliaser.GetAliasForEvent(message);

                var emailMessage =
                    new UserEmailNotificationInfo(
                        SUBJECT,
                        eventName,
                        user.UserName,
                        user.Email);

                await _smtpSender.SendNotificationAsync(emailMessage, token);

                var userTokens = _commonUnitOfWork
                    .UserDevicesRepository
                    .GetAllDevicesMapsAsync(token)
                    .GetAwaiter()
                    .GetResult()
                    .Where(x => x.UserId == userId && x.IsActive == true);

                await Task.WhenAll(
                    userTokens.Select(
                        device => Task.Run(
                            async () => await _adsSender.SendAdsPushNotificationAsync(
                                new(SUBJECT, user.UserName, eventName, device.FirebaseToken), token))));
            }

            _logger.LogInformation(
                "Going to slep {Seconds} seconds before next attempt",
                _configuration.DelaySeconds);

            await Task.Delay(_configuration.DelaySeconds * 1000, token);
        }
    }

    private const string SUBJECT = "System events synchronization";

    private readonly RedisReadingConfiguration _configuration;
    private readonly ICommonUsersUnitOfWork _commonUnitOfWork;
    private readonly ISMTPSender _smtpSender;
    private readonly IPushNotificationsSender _adsSender;
    private readonly IRedisMessagesReceiver _receiver;
    private readonly IRedisEventsAliaser _eventsAliaser;
    private readonly ILogger<RedisProcessor> _logger; 
}
