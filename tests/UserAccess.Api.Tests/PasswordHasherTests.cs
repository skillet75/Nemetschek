using UserAccess.Api.Infrastructure;
using Xunit;

namespace UserAccess.Api.Tests;

public sealed class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ProducesDifferentHashEachTime()
    {
        var password = "P@ssw0rd!";

        var hash1 = PasswordHasher.HashPassword(password);
        var hash2 = PasswordHasher.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
        Assert.True(PasswordHasher.VerifyPassword(password, hash1));
        Assert.True(PasswordHasher.VerifyPassword(password, hash2));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var hash = PasswordHasher.HashPassword("P@ssw0rd!");

        Assert.False(PasswordHasher.VerifyPassword("WrongPassword!", hash));
    }
}
