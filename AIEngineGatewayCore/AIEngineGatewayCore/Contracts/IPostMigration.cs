using AIEngineGateway.EngineInfrastructure;

namespace AIEngineGateway.Contracts
{
#nullable disable
    public interface IPostMigration
    {


        // Migration Name it should be unique followed by the TimeStamp.
        public string MigrationName { get; }

        Task ExecuteAsync(EngineContext engineContext);
    }
}
