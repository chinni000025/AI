using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIEngineCore.EngineCore
{
    public class EngineEventPublisher
    {
        private readonly IEngineBus _EngineBus;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEngineLatch _engineLatch;
        public EngineEventPublisher(IEngineBus engineBus, IServiceScopeFactory serviceScopeFactory,
            IEngineLatch engineLatch)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _EngineBus = engineBus;
            _engineLatch = engineLatch;
        }
        public async Task PublishEvent(EngineNotificationRequest @event, Priority priority = Priority.None, CancellationToken cancellationToken = default)
        {
            var EventId = Guid.NewGuid();
            var notificationId = Guid.NewGuid();
            EngineNotificationEvent engineEvent = new EngineNotificationEvent //preserve.
            {
                Id = EventId,
                EventData = _engineLatch.Serialize(@event),
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
            };
            EngineNotificationMessage engineNotificationMessage = new EngineNotificationMessage // notification to perform.
            {
                NotificationId = notificationId,
                EventId = EventId,
                Notification = @event.Notification,
                EngineEvents = @event.EngineEvents,
                NotificationPriority = @event.NotificationPriority
            };
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var engineNotificationEventService = scope.ServiceProvider.GetRequiredService<IEngineNotificationService>();
            await engineNotificationEventService.InsertEventNotification(engineEvent, cancellationToken);
            await _EngineBus.RouteAsync(engineNotificationMessage, cancellationToken);
        }
    }
}