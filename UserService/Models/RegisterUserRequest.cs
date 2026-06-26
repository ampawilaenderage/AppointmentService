namespace UserService.Models
{
    public class RegisterUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
        public string? Specialization { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
