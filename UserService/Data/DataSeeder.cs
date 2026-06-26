using Microsoft.EntityFrameworkCore;
using UserService.Models;
using UserService.Services;

namespace UserService.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Users.AnyAsync())
                return;

            var doctors = new[]
            {
                ("dr.james",    "James",    "Wilson",    "james.wilson@clinic.com",    "Cardiology",       "Male",   45),
                ("dr.sarah",    "Sarah",    "Chen",      "sarah.chen@clinic.com",      "Neurology",        "Female", 38),
                ("dr.michael",  "Michael",  "Brown",     "michael.brown@clinic.com",   "Orthopedics",      "Male",   52),
                ("dr.emily",    "Emily",    "Davis",     "emily.davis@clinic.com",     "Dermatology",      "Female", 41),
                ("dr.robert",   "Robert",   "Taylor",    "robert.taylor@clinic.com",   "Urology",          "Male",   49),
                ("dr.lisa",     "Lisa",     "Anderson",  "lisa.anderson@clinic.com",   "Gynecology",       "Female", 44),
                ("dr.david",    "David",    "Martinez",  "david.martinez@clinic.com",  "Gastroenterology", "Male",   56),
                ("dr.jennifer", "Jennifer", "White",     "jennifer.white@clinic.com",  "Pediatrics",       "Female", 36),
                ("dr.charles",  "Charles",  "Thompson",  "charles.thompson@clinic.com","Ophthalmology",    "Male",   60),
                ("dr.amanda",   "Amanda",   "Harris",    "amanda.harris@clinic.com",   "ENT",              "Female", 39),
                ("dr.kevin",    "Kevin",    "Lee",       "kevin.lee@clinic.com",       "Psychiatry",       "Male",   47),
                ("dr.patricia", "Patricia", "Clark",     "patricia.clark@clinic.com",  "General Practice", "Female", 53),
            };

            foreach (var (userId, first, last, email, spec, gender, age) in doctors)
            {
                var pw = PasswordHasher.HashPassword("Doc@123");
                db.Users.Add(new User
                {
                    UserId       = userId,
                    FirstName    = first,
                    LastName     = last,
                    FullName     = $"Dr. {first} {last}",
                    Email        = email,
                    PasswordHash = pw.Hash,
                    PasswordSalt = pw.Salt,
                    Role         = "Doctor",
                    Specialization = spec,
                    Gender       = gender,
                    Age          = age,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            var patients = new[]
            {
                ("john.smith",    "John",    "Smith",    "john.smith@mail.com",    "Male",   34),
                ("mary.johnson",  "Mary",    "Johnson",  "mary.johnson@mail.com",  "Female", 28),
                ("william.jones", "William", "Jones",    "william.jones@mail.com", "Male",   45),
                ("susan.miller",  "Susan",   "Miller",   "susan.miller@mail.com",  "Female", 52),
                ("thomas.wilson", "Thomas",  "Wilson",   "thomas.wilson@mail.com", "Male",   61),
            };

            foreach (var (userId, first, last, email, gender, age) in patients)
            {
                var pw = PasswordHasher.HashPassword("abc@321");
                db.Users.Add(new User
                {
                    UserId       = userId,
                    FirstName    = first,
                    LastName     = last,
                    FullName     = $"{first} {last}",
                    Email        = email,
                    PasswordHash = pw.Hash,
                    PasswordSalt = pw.Salt,
                    Role         = "Patient",
                    Gender       = gender,
                    Age          = age,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
