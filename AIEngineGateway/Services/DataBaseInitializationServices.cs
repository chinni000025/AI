using AIEngineGateway.EngineInfrastructure;
using AIEngineGateway.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static AIEngineConnectivity.Constants.EngineConstants;

namespace AIEngineGateway.Services
{
#nullable disable
    public class DataBaseIntialiationServices
    {
        public async Task TestConnectionAsync(string engineId, DataBaseProvider provider)
        {
            await provider.UseRequiredServer(engineId);
        }

        public async Task EnsureDatabaseExistsAsync(string connectionString, string databaseName, DataBaseProvider provider)
        {
            switch (provider)
            {
                case DataBaseProvider.PostgreSql:
                    await EnsurePostgresDatabaseExists(connectionString, databaseName);
                    break;
                default:
                    await EnsureSqlServerDatabaseExists(connectionString, databaseName);
                    break;
            }
        }

        private async Task EnsureSqlServerDatabaseExists(string connectionString, string databaseName)
        {
            await using var connection =
                new SqlConnection(connectionString);

            await connection.OpenAsync();

            var cmd = connection.CreateCommand();

            cmd.CommandText = $@"IF NOT EXISTS
                                    (SELECT name FROM sys.databases WHERE name = N'{databaseName}')
                                 BEGIN
                                        CREATE DATABASE[{databaseName}] 
                                 END";
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task EnsurePostgresDatabaseExists(string connectionString, string databaseName)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
            var exists = await checkCommand.ExecuteScalarAsync();

            if (exists == null)
            {
                await using var createCommand = connection.CreateCommand();

                createCommand.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                await createCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
