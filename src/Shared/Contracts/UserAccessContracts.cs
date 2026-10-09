using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "https://localhost";
    public string Audience { get; set; } = "UserAccess.Api";
    public string Key { get; set; } = "demo-local-development-signing-key-for-interview";
    public int ExpiryMinutes { get; set; } = 60;
}

public sealed record CreateUserRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters.")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters.")]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters.")]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$",
        ErrorMessage = "Password must contain at least one letter, one number, and one special character.")]
    public string Password { get; init; } = string.Empty;

    [StringLength(2048)]
    public string? Image { get; init; }
}

public record CreateTokenRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters.")]
    public string Password { get; init; } = string.Empty;
}

public record TokenRequest : CreateTokenRequest;
public record LoginRequest : CreateTokenRequest;
public record AuthTokenRequest : CreateTokenRequest;
public record AccessTokenRequest : CreateTokenRequest;

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Image,
    DateTime CreatedAtUtc);

public sealed record AuthTokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, string TokenType = "Bearer");
public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, string TokenType = "Bearer");
public sealed record AccessTokenResponse(string Token, DateTimeOffset ExpiresAtUtc, string TokenType = "Bearer");
