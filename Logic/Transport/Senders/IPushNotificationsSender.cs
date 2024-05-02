﻿using Models.UserActionModels.NotificationModels;

namespace Logic.Transport.Senders;

public interface IPushNotificationsSender
    : INotificationsSender
{
    Task SendAdsPushNotificationAsync(
        UserNotificationInfo model,
        CancellationToken token);
}
