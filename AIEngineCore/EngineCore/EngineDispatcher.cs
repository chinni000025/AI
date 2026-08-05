namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.ObjectModel;

    public sealed class EngineDispatcher : BackgroundService
    {
        private readonly IReadOnlyDictionary<Type, IEngineNotificationRouter> _Router;
        private readonly IEngineQueue<EngineNotification> _MainEngineQueue;

        public EngineDispatcher(IEnumerable<IEngineNotificationRouter> notifications, IEngineQueue<EngineNotification> engineQueue)
        {
            _Router = notifications.ToDictionary(r => r.EngineNotificationType);
            _MainEngineQueue = engineQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var engineNotifications in _MainEngineQueue.ReadAsync(cancellationToken))
            {
                if (_Router.TryGetValue(engineNotifications.Notification.GetType(), out var router))
                {
                    await router.publishAsync(engineNotifications, cancellationToken);
                }
            }
        }
    }
}