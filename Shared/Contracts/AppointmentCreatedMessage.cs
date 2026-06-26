namespace Shared.Contracts
{
    public class AppointmentCreatedMessage
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public DateTime Time { get; set; }
    }
}
