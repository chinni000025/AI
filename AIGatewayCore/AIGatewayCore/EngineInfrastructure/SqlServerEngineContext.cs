using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.EngineInfrastructure
{
    public class SqlServerEngineContext : EngineContext
    {
        public SqlServerEngineContext(DbContextOptions<SqlServerEngineContext> options) : base(options) { }
    }
}
