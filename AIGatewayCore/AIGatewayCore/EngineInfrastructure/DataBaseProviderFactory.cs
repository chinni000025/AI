using AIEngineGateway.Services;
using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.EngineInfrastructure
{
    public class DataBaseProviderFactory : IDataBaseProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public DataBaseProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Func<IDataBaseProvider> GetDataBaseProvider(EngineConstants.DataBaseProvider dataBaseProvider)
        {
            return dataBaseProvider switch
            {
                EngineConstants.DataBaseProvider.SqlServer =>
                    () => _serviceProvider.GetRequiredService<SqlServerProvider>(),

                EngineConstants.DataBaseProvider.PostgreSql =>
                    () => _serviceProvider.GetRequiredService<PostgreServerProvider>(),

                _ => throw new NotSupportedException()
            };
        }
    }
}
