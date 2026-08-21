using AIEngineGateway.Contracts;
using AIEngineGateway.EngineInfrastructure;

namespace AIEngineGateway.BackgroundServices.Jobs
{
    public class SoftDeleteCleanupJob : ICleanUpJob
    {
        public Task ExecuteAsync(EngineContext engineContext)
        {
            throw new NotImplementedException();
        }
    }
}