namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.ObjectModel;

    public sealed class EngineDispatcher
    {
        private readonly IReadOnlyDictionary<Type, IEngineNotificationRouter> _Router;

        public EngineDispatcher(IEnumerable<IEngineNotificationRouter> notifications)
        {
            _Router = notifications.ToDictionary(r => r.EngineNotificationType);
        }

        public async Task ExecuteAsync(IEngineQueue<IEngineNotification> mainEngineQueue, CancellationToken cancellationToken = default)
        {
            await foreach (var notification in mainEngineQueue.ReadAsync(cancellationToken))
            {
                if (_Router.TryGetValue(notification.GetType(), out var router))
                {
                    await router.publishAsync(notification, cancellationToken);
                }
            }
        }
    }
}