using AIEngineGateway.EngineInfrastructure;

namespace AIEngineGateway.Contracts
{
    public interface ICleanUpJob
    {
        Task ExecuteAsync(EngineContext engineContext);
    }
}
