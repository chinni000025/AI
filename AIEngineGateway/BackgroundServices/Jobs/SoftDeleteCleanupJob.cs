namespace AIEngineGateway.BackgroundServices.Jobs
{
    using AIEngineGateway.Contracts;
    using AIEngineGateway.EngineInfrastructure;

    public class SoftDeleteCleanupJob : ICleanUpJob
    {
        public Task ExecuteAsync(EngineContext engineContext)
        {
            throw new NotImplementedException();
        }
    }
}