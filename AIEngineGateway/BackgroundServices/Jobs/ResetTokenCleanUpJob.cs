using AIEngineGateway.Contracts;
using AIEngineGateway.EngineInfrastructure;

namespace AIEngineGateway.BackgroundServices.Jobs
{
    public class ResetTokenCleanUpJob : ICleanUpJob
    {

        public Task ExecuteAsync(EngineContext engineContext)
        {
            throw new NotImplementedException();
        }
    }
}
