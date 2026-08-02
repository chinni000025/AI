namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class EngineEmailRouter : IEngineNotificationRouter
    {
        private IEngineQueue<EngineEmailNotification> _EmailNotificationQueue;

        public EngineEmailRouter(IEngineQueue<EngineEmailNotification> emailNotificationQueue)
        {
            _EmailNotificationQueue = emailNotificationQueue;
        }

        public Type EngineNotificationType => typeof(EngineEmailNotification);

        public async ValueTask publishAsync(IEngineNotification notification, CancellationToken
            cancellationToken = default)
        {
            await _EmailNotificationQueue.publishAsync((EngineEmailNotification)notification, cancellationToken);
        }
    }
}