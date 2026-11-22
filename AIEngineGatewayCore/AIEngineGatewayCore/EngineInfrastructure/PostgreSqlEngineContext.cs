using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.EngineInfrastructure
{
    public class PostgreSqlEngineContext : EngineContext
    {
        public PostgreSqlEngineContext(DbContextOptions<PostgreSqlEngineContext> options) : base(options) { }
    }
}
