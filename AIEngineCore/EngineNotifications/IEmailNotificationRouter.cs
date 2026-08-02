namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class EmailNotificationRouter : INotificationRouter
    {
        private readonly IEngineQueue<EngineEmailNotification> _EmailNotificationQueue;
        public EmailNotificationRouter(IEngineQueue<EngineEmailNotification> emailNotificaionQueue)
        {
            _EmailNotificationQueue = emailNotificaionQueue;
        }
        public Type NotificationType => typeof(EngineEmailNotification);

        public async ValueTask RouterAsync(IEngineNotification notification, CancellationToken cancellationToken = default)
        {
            await _EmailNotificationQueue.publishAsync((EngineEmailNotification)notification, cancellationToken).AsTask();
        }
    }
}
