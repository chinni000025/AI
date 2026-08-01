namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IEngineNotificationProvider
    {
        void RegisterNotification(IEngineNotificationRegistry EngineNotificationRegistry);
    }
}