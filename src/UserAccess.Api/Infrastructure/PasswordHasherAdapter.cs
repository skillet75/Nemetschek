using UserAccess.Api.Application.Authentication;

namespace UserAccess.Api.Infrastructure;

internal sealed class PasswordHasherAdapter : IPasswordHasher
{
    public string HashPassword(string password) => PasswordHasher.HashPassword(password);

    public bool VerifyPassword(string password, string hashedPassword) =>
        PasswordHasher.VerifyPassword(password, hashedPassword);
}
