namespace NotificationService.Models
{
    public class ProcessedMessage
    {
        public int Id { get; set; }
        public Guid? MessageId { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public DateTime ProcessedAtUtc { get; set; }
        public string Status { get; set; } = "Processed";
        public string? Error { get; set; }
    }
}
