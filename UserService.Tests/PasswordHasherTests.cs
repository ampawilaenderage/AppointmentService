using FluentAssertions;
using UserService.Services;

namespace UserService.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ReturnsNonEmptyHashAndSalt()
    {
        var (hash, salt) = PasswordHasher.HashPassword("abc@321");

        hash.Should().NotBeNullOrWhiteSpace();
        salt.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void HashPassword_TwiceSamePassword_ProducesDifferentHashes()
    {
        var (hash1, _) = PasswordHasher.HashPassword("abc@321");
        var (hash2, _) = PasswordHasher.HashPassword("abc@321");

        // Each call uses a random salt, so hashes must differ
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var (hash, salt) = PasswordHasher.HashPassword("abc@321");

        var result = PasswordHasher.VerifyPassword("abc@321", hash, salt);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var (hash, salt) = PasswordHasher.HashPassword("abc@321");

        var result = PasswordHasher.VerifyPassword("wrongpassword", hash, salt);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_EmptyHash_ReturnsFalse()
    {
        var result = PasswordHasher.VerifyPassword("abc@321", "", "somesalt");

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_EmptySalt_ReturnsFalse()
    {
        var result = PasswordHasher.VerifyPassword("abc@321", "somehash", "");

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_TamperedHash_ReturnsFalse()
    {
        var (_, salt) = PasswordHasher.HashPassword("abc@321");

        var result = PasswordHasher.VerifyPassword("abc@321", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=", salt);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Doc@123")]
    [InlineData("abc@321")]
    [InlineData("P@$$w0rd!")]
    [InlineData("a")]
    public void VerifyPassword_RoundTrip_WorksForVariousPasswords(string password)
    {
        var (hash, salt) = PasswordHasher.HashPassword(password);

        var result = PasswordHasher.VerifyPassword(password, hash, salt);

        result.Should().BeTrue();
    }
}
