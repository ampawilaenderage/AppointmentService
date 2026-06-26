using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Data;
using NotificationService.Models;
using NotificationService.Services;
using Shared.Contracts;

namespace NotificationService.Consumers
{
    public class AppointmentCreatedConsumer : IConsumer<AppointmentCreatedMessage>
    {
        private readonly ILogger<AppointmentCreatedConsumer> _logger;
        private readonly NotificationProcessor _notifier;
        private readonly AppDbContext _db;

        public AppointmentCreatedConsumer(
            ILogger<AppointmentCreatedConsumer> logger,
            NotificationProcessor notifier,
            AppDbContext db)
        {
            _logger = logger;
            _notifier = notifier;
            _db = db;
        }

        public async Task Consume(ConsumeContext<AppointmentCreatedMessage> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                $"Received AppointmentCreated event: AppointmentId={msg.AppointmentId}, UserId={msg.UserId}, Time={msg.Time}");

            // Process notification (email, SMS, etc.)
            await _notifier.SendAppointmentCreatedNotification(msg);

            _db.ProcessedMessages.Add(new ProcessedMessage
            {
                MessageId = context.MessageId,
                MessageType = nameof(AppointmentCreatedMessage),
                AppointmentId = msg.AppointmentId,
                UserId = msg.UserId,
                AppointmentTime = msg.Time,
                ProcessedAtUtc = DateTime.UtcNow,
                Status = "Processed"
            });

            await _db.SaveChangesAsync();
        }
    }
}
