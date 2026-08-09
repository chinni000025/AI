namespace AIEngineGateway.BackgroundServices
{
    using AIEngineGateway.Contracts;
    using AIEngineGateway.EngineInfrastructure;

#nullable disable
    public class DataBaseCleanUpServices : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public DataBaseCleanUpServices(IServiceScopeFactory serviceProviderFactory)
        {
            _serviceScopeFactory = serviceProviderFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<EngineContext>();
                var jobs = scope.ServiceProvider.GetServices<ICleanUpJob>();
                try
                {
                    foreach (var job in jobs)
                    {
                        try
                        {
                            await job.ExecuteAsync(context);
                        }
                        catch
                        {
                            // will modify in future.
                        }
                    }
                }
                catch
                {
                    // will modify in future.
                }
            }
        }
    }
}