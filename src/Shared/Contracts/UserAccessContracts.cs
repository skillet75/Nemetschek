using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts;

public sealed record CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$",
        ErrorMessage = "Password must contain at least one letter, one number, and one special character.")]
    public string Password { get; init; } = string.Empty;

    [StringLength(2048)]
    public string? Image { get; init; }
}

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Image,
    DateTime CreatedAtUtc);
