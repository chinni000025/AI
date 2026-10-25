using AIEngineGateway.Extensions;
using Microsoft.Data.SqlClient;
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

        public async Task<bool> DataBaseExistsAsync(string connectionstring, string databaseName, DataBaseProvider dataBaseProvider)
        {
            switch (dataBaseProvider)
            {
                case DataBaseProvider.PostgreSql:
                    var builder = new NpgsqlConnectionStringBuilder(connectionstring);
                    if (string.IsNullOrWhiteSpace(builder.Database))
                    {
                        builder.Database = "postgres";
                    }
                    await using (var connection = new NpgsqlConnection(builder.ConnectionString))
                    {
                        await connection.OpenAsync();
                        await using var checkCommand = connection.CreateCommand();
                        checkCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
                        checkCommand.Parameters.AddWithValue("@name", databaseName);
                        var exists = await checkCommand.ExecuteScalarAsync();
                        return exists != null;
                    }
                default:
                    await using (var connection = new SqlConnection(connectionstring))
                    {
                        await connection.OpenAsync();
                        await using var cmd = connection.CreateCommand();
                        cmd.CommandText = "SELECT 1 FROM sys.databases WHERE name = @name";
                        cmd.Parameters.AddWithValue("@name", databaseName);
                        var exists = await cmd.ExecuteScalarAsync();
                        return exists != null;
                    }
            }
        }
    }
}
