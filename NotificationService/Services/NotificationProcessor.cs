using Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NotificationService.Services
{
    public class NotificationProcessor
    {
        private readonly ILogger<NotificationProcessor> _logger;

        public NotificationProcessor(ILogger<NotificationProcessor> logger)
        {
            _logger = logger;
        }

        public Task SendAppointmentCreatedNotification(AppointmentCreatedMessage message)
        {
            // 🔔 Here you can send:
            // - Email
            // - SMS
            // - Push notification
            // - Slack message
            // - Anything...

            _logger.LogInformation(
                $"[NotificationService] Sending notification for AppointmentId={message.AppointmentId} UserId={message.UserId}");

            return Task.CompletedTask;
        }
    }
}
