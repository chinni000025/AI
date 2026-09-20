using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;
using Quartz;

namespace AIEngineConnectivity.Services
{
    public interface IEngineScheduler
    {
        public Task ScheduleJobAsync(IJobDetail job, ITrigger trigger, CancellationToken ct = default);
        public Task DeleteJobAsync(JobKey jobKey, CancellationToken ct = default);
        public Task ScheduleEngineNotification(ScheduleEngineNotificationDTO scheduleEngineNotification,
            CancellationToken ct = default);
        public Task DeleteEngineRecycleItems(RecycleBin recycleBin, CancellationToken ct = default);
    }
}
