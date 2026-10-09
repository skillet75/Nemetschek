using UserAccess.Api.Infrastructure;
using Xunit;

namespace UserAccess.Api.Tests;

public sealed class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ProducesDifferentHashEachTime()
    {
        var password = "P@ssw0rd!";
        var passwordHasher = new PasswordHasher();

        var hash1 = passwordHasher.HashPassword(password);
        var hash2 = passwordHasher.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
        Assert.True(passwordHasher.VerifyPassword(password, hash1));
        Assert.True(passwordHasher.VerifyPassword(password, hash2));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var passwordHasher = new PasswordHasher();
        var hash = passwordHasher.HashPassword("P@ssw0rd!");

        Assert.False(passwordHasher.VerifyPassword("WrongPassword!", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("pbkdf2$abc$AA==$AA==")]
    [InlineData("pbkdf2$200000$%%%$AA==")]
    [InlineData("pbkdf2$200000$AA==$")]
    [InlineData("pbkdf2$200000$AQ==$AQ==")]
    [InlineData("pbkdf2$2000001$AAAAAAAAAAA=$AAAAAAAAAAAAAAAAAAAAAA==")]
    public void VerifyPassword_ReturnsFalseForMalformedHash(string malformedHash)
    {
        Assert.False(new PasswordHasher().VerifyPassword("P@ssw0rd!", malformedHash));
    }
}
