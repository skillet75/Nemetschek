namespace UserAccess.Api.Domain.Entities;

public sealed class User
{
    private User()
    {
    }

    public User(string firstName, string lastName, string email, string passwordHash, string? imagePath = null)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        ImagePath = imagePath;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? ImagePath { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}