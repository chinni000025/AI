namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Services;
    using Quartz;

    public class EngineScheduler : IEngineScheduler
    {
        private ISchedulerFactory _SchedulerFactory;
        public EngineScheduler(ISchedulerFactory schedulerFactory)
        {
            _SchedulerFactory = schedulerFactory;
        }

        public async Task ScheduleJobAsync(IJobDetail job, ITrigger trigger, CancellationToken ct = default)
        {
            var scheduler = await _SchedulerFactory.GetScheduler();
            var IsJobExists = await scheduler.CheckExists(job.Key, ct);

            if (IsJobExists)
                return;

            await scheduler.ScheduleJob(job, trigger, ct);
        }

        public async Task DeleteJobAsync(JobKey jobKey, CancellationToken ct = default)
        {
            var scheduler = await _SchedulerFactory.GetScheduler();
            var isJobExist = await scheduler.CheckExists(jobKey);
            if (!isJobExist)
                throw new Exception($"Job Not Exists with jobKey '{jobKey}'");

            await scheduler.DeleteJob(jobKey, ct);
        }
    }
}
