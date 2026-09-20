using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Services;
using Quartz;

namespace AIEngineGateway.BackgroundServices.Jobs
{
    public class EngineRecycleBinJob(ILogger<EngineRecycleBinJob> logger,
        IServiceScopeFactory serviceScopeFactory) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var recycleBinRawId = context.MergedJobDataMap.GetString("RecycleBinId");
            var itemTypeRaw = context.MergedJobDataMap.GetString("RecycleItem");
            var entityRawId = context.MergedJobDataMap.GetString("EntityId");

            if (!Guid.TryParse(recycleBinRawId, out var recycleId) ||
                !Guid.TryParse(entityRawId, out var entityId) ||
                !Enum.TryParse(itemTypeRaw, out RecycleItem recycleItem))
            {
                logger.LogError("Recycle job data invalid. EntityId={EntityId}", recycleBinRawId);
                return;
            }
            var service = scope.ServiceProvider.GetRequiredService<IEngineRecycleBinService>();
            await service.DeleteRecycleItem(recycleId, recycleItem, entityId, context.CancellationToken);
        }
    }
}