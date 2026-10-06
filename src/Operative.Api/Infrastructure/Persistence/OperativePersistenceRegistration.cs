using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Operative.Api.Infrastructure.Persistence;

public static class OperativePersistenceRegistration
{
    public static IServiceCollection AddOperativePersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("The operative database connection string is not configured.");
        var sqliteConnection = new SqliteConnectionStringBuilder(connectionString);

        if (!string.Equals(sqliteConnection.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase) &&
            !sqliteConnection.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            var databasePath = Path.GetFullPath(sqliteConnection.DataSource, environment.ContentRootPath);
            var databaseDirectory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(databaseDirectory))
            {
                Directory.CreateDirectory(databaseDirectory);
            }

            sqliteConnection.DataSource = databasePath;
        }

        services.AddDbContext<OperativeDbContext>(options => options.UseSqlite(sqliteConnection.ToString()));
        return services;
    }
}