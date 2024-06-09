using Logic.Abstractions;

using Microsoft.Extensions.Logging;

using PostgreSQL.Abstractions;

namespace Logic;

public sealed class UserDevicesTokensActualizer :
    IUserDevicesTokensActualizer
{
    public UserDevicesTokensActualizer(
        ICommonUsersUnitOfWork userUnitOfWork,
        ILogger<UserDevicesTokensActualizer> logger)
    {
        _usersUnitOfWork = userUnitOfWork 
            ?? throw new ArgumentNullException(nameof(userUnitOfWork));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ActualizeUserDevicesAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Beginning next iteration of checking expired" +
                " user tokens after {NumberDays} days",
                EXPIRED_TIME.Days);

            var users = await _usersUnitOfWork.UsersRepository.GetAllUsersAsync(token);

            var allTokens =
                await _usersUnitOfWork.UserDevicesRepository.GetAllDevicesMapsAsync(token);

            var now = DateTimeOffset.UtcNow;

            foreach (var user in users) 
            {
                var userTokens =
                    allTokens
                    .Where(
                        x => x.UserId == user.Id
                        && now.Subtract(x.TokenSetMoment) > EXPIRED_TIME)
                    .ToList();

                foreach (var device in userTokens)
                {
                    await _usersUnitOfWork.UserDevicesRepository
                        .DeleteAsync(user.Id, device.FirebaseToken, token);
                }
            }

            _usersUnitOfWork.SaveChanges();

            await Task.Delay(DELAY);
        }
    }

    private static readonly TimeSpan EXPIRED_TIME = TimeSpan.FromDays(7);
    private static readonly TimeSpan DELAY = TimeSpan.FromMinutes(60);

    private readonly ICommonUsersUnitOfWork _usersUnitOfWork;
    private readonly ILogger _logger;
}
