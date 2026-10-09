using Shared.Contracts;
using UserAccess.Api.Application.Authentication;
using UserAccess.Api.Domain.Entities;

namespace UserAccess.Api.Application.Registration;

public sealed class UserRegistrationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
{
    public async Task<UserResponse?> RegisterAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        if (await userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return null;
        }

        ImageDataUri.TryParse(request.Image, out var image);
        var user = new User(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            normalizedEmail,
            passwordHasher.HashPassword(request.Password),
            image?.Data,
            image?.MediaType);

        if (!await userRepository.AddAsync(user, cancellationToken))
        {
            return null;
        }
        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            ToImageDataUri(user),
            user.CreatedAtUtc);
    }

    private static string? ToImageDataUri(User user) => user.ImageData is null || user.ImageContentType is null
        ? null
        : $"data:{user.ImageContentType};base64,{Convert.ToBase64String(user.ImageData)}";
}
