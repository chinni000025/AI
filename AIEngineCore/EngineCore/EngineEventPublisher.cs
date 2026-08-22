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
        public async Task PublishEvent(EngineNotificationMessage @event, CancellationToken cancellationToken = default)
        {
            var EventId = Guid.NewGuid();
            EngineNotificationEvent engineNotificationEvent = new EngineNotificationEvent
            {
                Id = EventId,
                EventData = _engineLatch.Serialize(@event),
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            @event.EventId = EventId;
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var engineNotificationEventService = scope.ServiceProvider.GetRequiredService<IEngineNotificationService>();
            await engineNotificationEventService.InsertEventNotification(engineNotificationEvent, cancellationToken);
            await _EngineBus.RouteAsync(@event, cancellationToken);
        }
    }
}