namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class DefaultEngineNotificationProvider : IEngineNotificationProvider
    {
        public void RegisterNotification(IEngineNotificationRegistry registry)
        {
            registry.addOrUpdateNotifications(EngineEvents.UserCreated, new EngineEmailNotification());
            registry.addOrUpdateNotifications(EngineEvents.ForgetPassword, new EngineEmailNotification());
        }
    }
}