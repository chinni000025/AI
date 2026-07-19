namespace AIEngineGateway.Contracts
{
    using AIEngineGateway.EngineInfrastructure;

    public interface ICleanUpJob
    {
        Task ExecuteAsync(EngineContext engineContext);
    }
}
