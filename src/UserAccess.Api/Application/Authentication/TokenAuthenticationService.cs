using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts;

namespace UserAccess.Api.Application.Authentication;

public sealed class TokenAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly IPasswordHasher _passwordHasher;

    public TokenAuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthTokenResponse?> AuthenticateAsync(
        CreateTokenRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        return new AuthTokenResponse(CreateToken(user.Id, user.Email, user.FirstName, user.LastName, expiresAtUtc), expiresAtUtc);
    }

    private string CreateToken(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        DateTimeOffset expiresAtUtc)
    {
        var key = string.IsNullOrWhiteSpace(_jwtSettings.Key)
            ? "demo-local-development-signing-key-for-interview"
            : _jwtSettings.Key;

        var signingKeyBytes = Encoding.UTF8.GetBytes(key);
        var keyMaterial = signingKeyBytes.Length >= 32
            ? signingKeyBytes
            : SHA256.HashData(signingKeyBytes);

        var signingKey = new SymmetricSecurityKey(keyMaterial);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, firstName + " " + lastName)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
