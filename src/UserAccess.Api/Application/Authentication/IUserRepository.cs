using UserAccess.Api.Domain.Entities;

namespace UserAccess.Api.Application.Authentication;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}
