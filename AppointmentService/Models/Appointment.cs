namespace AppointmentService.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Time { get; set; }
        public int? DoctorId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string ProblemType { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
    }
}
