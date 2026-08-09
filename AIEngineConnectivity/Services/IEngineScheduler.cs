namespace AIEngineConnectivity.Services
{
    using Quartz;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IEngineScheduler
    {
        public Task ScheduleJobAsync(IJobDetail job, ITrigger trigger, CancellationToken ct = default);
        public Task DeleteJobAsync(JobKey jobKey, CancellationToken ct = default);
    }
}
