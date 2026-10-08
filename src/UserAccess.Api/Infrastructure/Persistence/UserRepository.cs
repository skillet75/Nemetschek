using Microsoft.EntityFrameworkCore;
using UserAccess.Api.Application.Authentication;
using UserAccess.Api.Domain.Entities;

namespace UserAccess.Api.Infrastructure.Persistence;

internal sealed class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
}
