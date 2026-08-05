namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class EngineEmailRouter : IEngineNotificationRouter
    {
        private IEngineQueue<EngineNotification> _EmailNotificationQueue;

        public EngineEmailRouter(IEngineQueue<EngineNotification> emailNotificationQueue)
        {
            _EmailNotificationQueue = emailNotificationQueue;
        }

        public Type EngineNotificationType => typeof(EngineEmailNotification);

        public async ValueTask publishAsync(EngineNotification notification, CancellationToken
            cancellationToken = default)
        {
            await _EmailNotificationQueue.publishAsync(notification, cancellationToken);
        }
    }
}