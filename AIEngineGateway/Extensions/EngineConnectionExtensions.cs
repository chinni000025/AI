namespace AIEngineGateway.Extensions
{
    using Microsoft.Data.SqlClient;
    using Npgsql;
    using static AIEngineConnectivity.Constants.EngineConstants;
    public static class EngineConnectionExtensions
    {
        public static async Task UseRequiredServer(this DataBaseProvider databaseProvider, string connectionString)
        {
            switch (databaseProvider)
            {
                case DataBaseProvider.SqlServer:

                    await using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                    }

                    break;

                case DataBaseProvider.PostgreSql:
                    await using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                    }

                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
