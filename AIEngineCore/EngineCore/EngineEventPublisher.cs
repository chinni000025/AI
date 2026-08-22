using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Services;

namespace AIEngineCore.EngineCore
{
    public class EngineEventPublisher
    {
        private readonly IEngineBus _EngineBus;
        //private readonly IEngineNotificationEventService _engineNotificationEventService;
        private readonly IEngineLatch _engineLatch;
        public EngineEventPublisher(IEngineBus engineBus,
            IEngineLatch engineLatch)
        {
            _EngineBus = engineBus;
            _engineLatch = engineLatch;
        }
        public async Task PublishEvent(EngineNotificationMessage @event, CancellationToken cancellationToken = default)
        {
            //var EventId = Guid.NewGuid();
            //EngineNotificationEvent engineNotificationEvent = new EngineNotificationEvent
            //{
            //    Id = EventId,
            //    EventType = @event.EngineEvents.Value,
            //    EventData = _engineLatch.Serialize(@event),
            //    CreatedAt = DateTime.UtcNow,
            //    ModifiedAt = DateTime.UtcNow
            //};
            //@event.EventId = EventId;
            //await _engineNotificationEventService.InsertEventNotification(engineNotificationEvent, cancellationToken);
            await _EngineBus.RouteAsync(@event, cancellationToken);
        }
    }
}