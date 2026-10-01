using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Orama.Infra.Data.Context;
using Testcontainers.PostgreSql;
using Xunit;

namespace Orama.Tests.Integration;

public sealed class RequiresPostgresFactAttribute : FactAttribute
{
    public RequiresPostgresFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("ORAMA_RUN_POSTGRES_TESTS"), "1", StringComparison.Ordinal))
            Skip = "Defina ORAMA_RUN_POSTGRES_TESTS=1 em ambiente com Docker para executar PostgreSQL/Testcontainers.";
    }
}

public sealed class PostgreSqlHttpSmokeTests
{
    [RequiresPostgresFact]
    public async Task Health_endpoint_and_EF_connection_work_against_ephemeral_PostgreSQL()
    {
        await using var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("orama_test")
            .WithUsername("orama_test")
            .WithPassword("test-password-only")
            .Build();
        await postgres.StartAsync();

        await using var factory = new TestApplicationFactory(postgres.GetConnectionString());
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        Assert.Contains("Healthy", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OramaDbContext>();
        Assert.True(await db.Database.CanConnectAsync());
    }

    private sealed class TestApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = connectionString,
                    ["Database:Provider"] = "PostgreSql",
                    ["Jwt:Key"] = new string('k', 48),
                    ["Jwt:Issuer"] = "Orama.Tests",
                    ["Jwt:Audience"] = "Orama.Tests"
                }));
        }
    }
}
