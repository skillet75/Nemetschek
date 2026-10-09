using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace UserAccess.Api.Tests;

public sealed class UserAccessApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"user-access-tests-{Guid.NewGuid():N}.db");
    public string DatabasePath => _databasePath;
    public string ConnectionString => $"Data Source={_databasePath};Pooling=False";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Development");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Jwt:Key"] = "integration-test-jwt-signing-key-at-least-32-bytes",
                ["Jwt:Issuer"] = "https://localhost",
                ["Jwt:Audience"] = "UserAccess.Api"
            }));

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        finally
        {
            if (disposing)
            {
                DeleteDatabaseFiles();
            }
        }
    }

    private void DeleteDatabaseFiles()
    {
        foreach (var path in new[] { _databasePath, $"{_databasePath}-shm", $"{_databasePath}-wal", $"{_databasePath}-journal" })
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
