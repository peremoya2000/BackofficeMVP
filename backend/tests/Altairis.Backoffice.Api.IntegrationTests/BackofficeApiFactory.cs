using Altairis.Backoffice.Api.IntegrationTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(BackofficeApiFactory))]

namespace Altairis.Backoffice.Api.IntegrationTests;

public sealed class BackofficeApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string POSTGRES_IMAGE = "postgres:18-alpine";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder(POSTGRES_IMAGE).Build();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Backoffice", _database.GetConnectionString());
    }
}
