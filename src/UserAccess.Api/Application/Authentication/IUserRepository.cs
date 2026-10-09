using UserAccess.Api.Domain.Entities;

namespace UserAccess.Api.Application.Authentication;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<bool> AddAsync(User user, CancellationToken cancellationToken);
}
