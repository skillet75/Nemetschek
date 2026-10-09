using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using UserAccess.Api.Application.Authentication;
using UserAccess.Api.Domain.Entities;

namespace UserAccess.Api.Infrastructure.Persistence;

internal sealed class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public async Task<bool> AddAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsEmailUniqueViolation(exception))
        {
            return false;
        }
    }

    private static bool IsEmailUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException
        {
            SqliteErrorCode: 19,
            SqliteExtendedErrorCode: 2067
        } sqliteException &&
        sqliteException.Message.Contains("Users.Email", StringComparison.OrdinalIgnoreCase);
}
