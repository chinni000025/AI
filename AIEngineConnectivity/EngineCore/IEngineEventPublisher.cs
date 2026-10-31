using AIEngineConnectivity.Models;

namespace AIEngineConnectivity.EngineCore
{
    public interface IEngineEventPublisher
    {
        Task PublishEvent(EngineNotificationRequest @event, Priority priority = Priority.None, CancellationToken cancellationToken = default);
    }
}