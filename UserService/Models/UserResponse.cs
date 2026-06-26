namespace UserService.Models
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
