using AIEngineConnectivity.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IEngineNotificationEventService
    {
        public Task InsertEventNotification(EngineNotificationEvent engineNotificationEvent, CancellationToken cancellationToken);
    }
}
