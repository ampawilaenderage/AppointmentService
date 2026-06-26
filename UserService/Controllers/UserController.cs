using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using UserService.Data;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPublishEndpoint _publish;

        public UserController(AppDbContext db, IPublishEndpoint publish)
        {
            _db = db;
            _publish = publish;
        }

        [HttpGet]
        public async Task<IEnumerable<UserResponse>> GetAll() =>
            await _db.Users.Select(u => ToResponse(u)).ToListAsync();

        [HttpGet("doctors")]
        public async Task<IEnumerable<UserResponse>> GetDoctors([FromQuery] string? specialization)
        {
            var query = _db.Users.Where(u => u.Role == "Doctor");
            if (!string.IsNullOrWhiteSpace(specialization))
                query = query.Where(u => u.Specialization == specialization);
            return await query.Select(u => ToResponse(u)).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null) return NotFound();
            return Ok(ToResponse(user));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterUserRequest request)
        {
            var userId = request.UserId.Trim();
            var email = request.Email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("User id, first name, last name, email, and password are required.");
            }

            if (await _db.Users.AnyAsync(x => x.UserId == userId))
                return Conflict("User id already exists.");

            if (await _db.Users.AnyAsync(x => x.Email == email))
                return Conflict("Email already exists.");

            var validRoles = new[] { "Patient", "Doctor" };
            var role = string.IsNullOrWhiteSpace(request.Role) ? "Patient" : request.Role.Trim();
            if (!validRoles.Contains(role))
                return BadRequest("Role must be Patient or Doctor.");

            var pw = PasswordHasher.HashPassword(request.Password);
            var firstName = request.FirstName.Trim();
            var lastName = request.LastName.Trim();

            var user = new User
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                Email = email,
                PasswordHash = pw.Hash,
                PasswordSalt = pw.Salt,
                Role = role,
                Specialization = role == "Doctor" ? request.Specialization?.Trim() : null,
                Gender = request.Gender.Trim(),
                Age = request.Age,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await _publish.Publish(new UserCreatedMessage
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName
            });

            return Ok(ToResponse(user));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var userId = request.UserId.Trim();
            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == userId);

            if (user is null ||
                !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized("Invalid user id or password.");
            }

            return Ok(ToResponse(user));
        }

        private static UserResponse ToResponse(User user) => new()
        {
            Id = user.Id,
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Specialization = user.Specialization,
            Gender = user.Gender,
            Age = user.Age,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
