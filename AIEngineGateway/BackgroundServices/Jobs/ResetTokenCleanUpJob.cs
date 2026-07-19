namespace AIEngineGateway.BackgroundServices.Jobs
{
    using AIEngineGateway.Contracts;
    using AIEngineGateway.EngineInfrastructure;

    public class ResetTokenCleanUpJob : ICleanUpJob
    {

        public Task ExecuteAsync(EngineContext engineContext)
        {
            throw new NotImplementedException();
        }
    }
}
