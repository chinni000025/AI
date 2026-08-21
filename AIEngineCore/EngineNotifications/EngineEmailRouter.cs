using AIEngineConnectivity.EngineCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineCore.EngineNotifications
{
    public class EngineEmailRouter : IEngineNotificationRouter
    {
        private IEngineQueue<EngineNotificationMessage> _EmailNotificationQueue;

        public EngineEmailRouter(IEngineQueue<EngineNotificationMessage> emailNotificationQueue)
        {
            _EmailNotificationQueue = emailNotificationQueue;
        }

        public Type EngineNotificationType => typeof(EngineEmailNotification);

        public async ValueTask publishAsync(EngineNotificationMessage notification, CancellationToken
            cancellationToken = default)
        {
            await _EmailNotificationQueue.publishAsync(notification, cancellationToken);
        }
    }
}