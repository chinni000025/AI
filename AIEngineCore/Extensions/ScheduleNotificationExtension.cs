using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Services;
using System;

namespace AIEngineCore.Extensions
{
#nullable disable

    public static class ScheduleNotificationExtension
    {
        public static async Task ScheduleNotification(this IEngineScheduler engineScheduler,
            EngineNotificationMessage engineNotification, DateTime retryAt,
            NotificationType notificationType, CancellationToken cancellationToken)
        {
            await engineScheduler.ScheduleEngineNotification(new ScheduleEngineNotificationDTO
            {
                NotificationType = NotificationType.EmailNotification,
                RetryAt = retryAt,
                NotificationId = engineNotification.NotificationId.Value
            }, cancellationToken);
        }
    }
}