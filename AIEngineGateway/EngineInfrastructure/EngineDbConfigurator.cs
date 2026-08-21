using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using static AIEngineConnectivity.Constants.EngineConstants;

namespace AIEngineGateway.EngineInfrastructure
{
    public class EngineDbConfigurator
    {
        public void ConfigureEngineDataBase(IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder, DataBaseProvider dataBaseProvider)
        {
            var engineConfig = serviceProvider.GetRequiredService<EngineConfig>();
            if (!engineConfig.IsEngineConfig() || engineConfig.GetDatabaseType() != dataBaseProvider)
            {
                if (dataBaseProvider is DataBaseProvider.SqlServer)
                {
                    dbContextOptionsBuilder.UseSqlServer("Server=INITAL_SETUP;Database=PENDING;User Id=PENDING;Password=PENDING;",
                 sqlOptions => sqlOptions.MigrationsAssembly(typeof(SqlServerEngineContext).Assembly.FullName));
                }
                else
                {
                    dbContextOptionsBuilder.UseNpgsql("Host=INITIAL_SETUP;Database=PENDING;Username=PENDING;Password=PENDING;", postgresOptions =>
                    postgresOptions.MigrationsAssembly(typeof(PostgreSqlEngineContext).Assembly.FullName));
                }
            }
            else
            {
                var connectionString = engineConfig.ConnectionString();
                switch (dataBaseProvider)
                {
                    case DataBaseProvider.SqlServer:
                        dbContextOptionsBuilder.UseSqlServer(connectionString
                            , sqlOptions => sqlOptions.MigrationsAssembly(typeof(SqlServerEngineContext).Assembly.FullName));
                        break;
                    case DataBaseProvider.PostgreSql:
                        dbContextOptionsBuilder.UseNpgsql(connectionString,
                            postgresOptions => postgresOptions.MigrationsAssembly(typeof(PostgreSqlEngineContext).Assembly.FullName));
                        break;
                    default:
                        throw new Exception("No such Server available");
                }
            }
        }
    }
}
