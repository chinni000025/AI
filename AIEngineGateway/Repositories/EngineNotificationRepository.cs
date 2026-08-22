using AIEngineGateway.EngineInfrastructure;

namespace AIEngineGateway.Repositories
{
    public class EngineNotificationRepository
    {
        private EngineContext _engineContext;

        public EngineNotificationRepository(EngineContext engineContext)
        {
            _engineContext = engineContext;
        }

        public async Task GetNotificationByPriority()
        {
            //var query = from e in _engineContext.EngineNotificationEvents
        }
    }
}
