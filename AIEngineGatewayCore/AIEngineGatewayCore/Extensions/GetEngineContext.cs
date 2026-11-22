using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;
using static AIEngineConnectivity.Constants.EngineConstants;

namespace AIEngineGateway.Extensions
{
    public static class GetEngineContext
    {
        public static EngineContext GetEngineContextOptions(this DataBaseProvider dataBaseProvider, string connectionString)
        {
            switch (dataBaseProvider)
            {
                case DataBaseProvider.SqlServer:
                    var sqlBuilder = new DbContextOptionsBuilder<SqlServerEngineContext>();
                    sqlBuilder.UseSqlServer(connectionString,
                        sqlOptions => sqlOptions.MigrationsAssembly(typeof(SqlServerEngineContext).Assembly.FullName));

                    return new SqlServerEngineContext(sqlBuilder.Options);

                case DataBaseProvider.PostgreSql:
                    var postgresBuilder = new DbContextOptionsBuilder<PostgreSqlEngineContext>();
                    postgresBuilder.UseNpgsql(connectionString,
                        postgresOptions => postgresOptions.MigrationsAssembly(typeof(PostgreSqlEngineContext).Assembly.FullName));

                    return new PostgreSqlEngineContext(postgresBuilder.Options);
                default:
                    throw new Exception("No such Server Exists");
            }
        }
    }
}
