using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using UserService.Controllers;
using UserService.Models;
using UserService.Tests.Helpers;

namespace UserService.Tests;

public class UserControllerTests
{
    private static UserController CreateController(string dbName)
    {
        var db = TestDbContext.Create(dbName);
        var publish = Substitute.For<IPublishEndpoint>();
        return new UserController(db, publish);
    }

    // ── Register / Create ────────────────────────────────────────

    [Fact]
    public async Task Create_ValidPatient_Returns200WithUser()
    {
        var controller = CreateController(nameof(Create_ValidPatient_Returns200WithUser));

        var result = await controller.Create(new RegisterUserRequest
        {
            UserId = "john01", FirstName = "John", LastName = "Smith",
            Email = "john@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Male", Age = 30
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var user = ok.Value.Should().BeOfType<UserResponse>().Subject;
        user.UserId.Should().Be("john01");
        user.Role.Should().Be("Patient");
    }

    [Fact]
    public async Task Create_ValidDoctor_StoresSpecialization()
    {
        var controller = CreateController(nameof(Create_ValidDoctor_StoresSpecialization));

        var result = await controller.Create(new RegisterUserRequest
        {
            UserId = "doc01", FirstName = "Alice", LastName = "Wong",
            Email = "alice@test.com", Password = "Doc@123",
            Role = "Doctor", Specialization = "Cardiology",
            Gender = "Female", Age = 40
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var user = ok.Value.Should().BeOfType<UserResponse>().Subject;
        user.Specialization.Should().Be("Cardiology");
    }

    [Fact]
    public async Task Create_PatientWithSpecialization_IgnoresSpecialization()
    {
        var controller = CreateController(nameof(Create_PatientWithSpecialization_IgnoresSpecialization));

        var result = await controller.Create(new RegisterUserRequest
        {
            UserId = "pat01", FirstName = "Bob", LastName = "Lee",
            Email = "bob@test.com", Password = "abc@321",
            Role = "Patient", Specialization = "Cardiology",
            Gender = "Male", Age = 25
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var user = ok.Value.Should().BeOfType<UserResponse>().Subject;
        user.Specialization.Should().BeNull();
    }

    [Fact]
    public async Task Create_DuplicateUserId_Returns409()
    {
        var controller = CreateController(nameof(Create_DuplicateUserId_Returns409));
        var request = new RegisterUserRequest
        {
            UserId = "dup01", FirstName = "A", LastName = "B",
            Email = "a@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Male", Age = 20
        };

        await controller.Create(request);
        var result = await controller.Create(new RegisterUserRequest
        {
            UserId = "dup01", FirstName = "C", LastName = "D",
            Email = "c@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Female", Age = 22
        });

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        var controller = CreateController(nameof(Create_DuplicateEmail_Returns409));

        await controller.Create(new RegisterUserRequest
        {
            UserId = "usr01", FirstName = "A", LastName = "B",
            Email = "same@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Male", Age = 20
        });

        var result = await controller.Create(new RegisterUserRequest
        {
            UserId = "usr02", FirstName = "C", LastName = "D",
            Email = "same@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Female", Age = 22
        });

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidRole_Returns400()
    {
        var controller = CreateController(nameof(Create_InvalidRole_Returns400));

        var result = await controller.Create(new RegisterUserRequest
        {
            UserId = "usr03", FirstName = "A", LastName = "B",
            Email = "x@test.com", Password = "abc@321",
            Role = "Admin", Gender = "Male", Age = 20
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("", "First", "Last", "e@t.com", "pass")]
    [InlineData("usr", "", "Last", "e@t.com", "pass")]
    [InlineData("usr", "First", "", "e@t.com", "pass")]
    [InlineData("usr", "First", "Last", "e@t.com", "")]
    public async Task Create_MissingRequiredFields_Returns400(
        string userId, string firstName, string lastName, string email, string password)
    {
        var controller = CreateController($"Create_MissingFields_{userId}_{firstName}");

        var result = await controller.Create(new RegisterUserRequest
        {
            UserId = userId, FirstName = firstName, LastName = lastName,
            Email = email, Password = password,
            Role = "Patient", Gender = "Male", Age = 20
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Login ─────────────────────────────────────────────────────

    [Fact]
    public async Task Login_CorrectCredentials_Returns200WithUser()
    {
        var controller = CreateController(nameof(Login_CorrectCredentials_Returns200WithUser));
        await controller.Create(new RegisterUserRequest
        {
            UserId = "john02", FirstName = "John", LastName = "Smith",
            Email = "john2@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Male", Age = 30
        });

        var result = await controller.Login(new LoginRequest { UserId = "john02", Password = "abc@321" });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var user = ok.Value.Should().BeOfType<UserResponse>().Subject;
        user.UserId.Should().Be("john02");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var controller = CreateController(nameof(Login_WrongPassword_Returns401));
        await controller.Create(new RegisterUserRequest
        {
            UserId = "john03", FirstName = "John", LastName = "Smith",
            Email = "john3@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Male", Age = 30
        });

        var result = await controller.Login(new LoginRequest { UserId = "john03", Password = "wrongpass" });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        var controller = CreateController(nameof(Login_NonExistentUser_Returns401));

        var result = await controller.Login(new LoginRequest { UserId = "nobody", Password = "abc@321" });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_PasswordHashNotExposedInResponse()
    {
        var controller = CreateController(nameof(Login_PasswordHashNotExposedInResponse));
        await controller.Create(new RegisterUserRequest
        {
            UserId = "john04", FirstName = "John", LastName = "Smith",
            Email = "john4@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Male", Age = 30
        });

        var result = await controller.Login(new LoginRequest { UserId = "john04", Password = "abc@321" });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<UserResponse>().Subject;
        // UserResponse must never expose PasswordHash or PasswordSalt properties
        typeof(UserResponse).GetProperty("PasswordHash").Should().BeNull(
            "password hash must not be returned to the client");
        typeof(UserResponse).GetProperty("PasswordSalt").Should().BeNull(
            "password salt must not be returned to the client");
        response.Should().NotBeNull();
    }

    // ── GetDoctors ────────────────────────────────────────────────

    [Fact]
    public async Task GetDoctors_NoFilter_ReturnsOnlyDoctors()
    {
        var controller = CreateController(nameof(GetDoctors_NoFilter_ReturnsOnlyDoctors));
        await controller.Create(new RegisterUserRequest
        {
            UserId = "doc10", FirstName = "Dr", LastName = "A",
            Email = "doca@test.com", Password = "Doc@123",
            Role = "Doctor", Specialization = "Cardiology", Gender = "Male", Age = 40
        });
        await controller.Create(new RegisterUserRequest
        {
            UserId = "pat10", FirstName = "Pat", LastName = "B",
            Email = "patb@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Female", Age = 25
        });

        var doctors = await controller.GetDoctors(null);

        doctors.Should().HaveCount(1);
        doctors.First().Role.Should().Be("Doctor");
    }

    [Fact]
    public async Task GetDoctors_WithSpecialization_FiltersCorrectly()
    {
        var controller = CreateController(nameof(GetDoctors_WithSpecialization_FiltersCorrectly));
        await controller.Create(new RegisterUserRequest
        {
            UserId = "doc20", FirstName = "Dr", LastName = "Heart",
            Email = "heart@test.com", Password = "Doc@123",
            Role = "Doctor", Specialization = "Cardiology", Gender = "Male", Age = 45
        });
        await controller.Create(new RegisterUserRequest
        {
            UserId = "doc21", FirstName = "Dr", LastName = "Skin",
            Email = "skin@test.com", Password = "Doc@123",
            Role = "Doctor", Specialization = "Dermatology", Gender = "Female", Age = 38
        });

        var result = await controller.GetDoctors("Cardiology");

        result.Should().HaveCount(1);
        result.First().Specialization.Should().Be("Cardiology");
    }

    [Fact]
    public async Task GetDoctors_NoMatchingSpecialization_ReturnsEmpty()
    {
        var controller = CreateController(nameof(GetDoctors_NoMatchingSpecialization_ReturnsEmpty));

        var result = await controller.GetDoctors("Neurology");

        result.Should().BeEmpty();
    }

    // ── GetById ───────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        var controller = CreateController(nameof(GetById_ExistingUser_ReturnsUser));
        await controller.Create(new RegisterUserRequest
        {
            UserId = "usr50", FirstName = "Sam", LastName = "Lee",
            Email = "sam@test.com", Password = "abc@321",
            Role = "Patient", Gender = "Male", Age = 28
        });
        var all = await controller.GetAll();
        var id = all.First().Id;

        var result = await controller.GetById(id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<UserResponse>()
            .Which.UserId.Should().Be("usr50");
    }

    [Fact]
    public async Task GetById_NonExistingUser_Returns404()
    {
        var controller = CreateController(nameof(GetById_NonExistingUser_Returns404));

        var result = await controller.GetById(9999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
