using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Infrastructure.Identity;
using RealEstateApp.Infrastructure.Identity.Contexts;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using Testcontainers.MsSql;

namespace RealEstateApp.Api.Tests.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithCleanUp(true)
        .Build();

    public HttpClient Client { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:RealEstateDb"] = _container.GetConnectionString(),
                        ["JwtSettings:SecretKey"] =
                            "ac51b1d53ca4154b6b5fc7258ee9bb1bc04b17941a5823fe8bb2b7f38249891ce10b3860ae4b5e982412a7ba1ae05971bb8bd94a6b03b2ce8f6af6d41b1fe834",
                        ["JwtSettings:Issuer"] = "RealEstateApp_P3_Api",
                        ["JwtSettings:Audience"] = "RealEstate_Users",
                        ["JwtSettings:DurationInMinutes"] = "120",
                    }
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            services.AddIdentityBackendUseCases();

            // Aplicamos las migraciones de la base de datos para asegurarnos
            // de que la base de datos esté en el estado correcto antes de ejecutar las pruebas.
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var inner = scope.ServiceProvider;
            inner.GetRequiredService<AppDbContext>().Database.Migrate();
            inner.GetRequiredService<IdentityContext>().Database.Migrate();
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Client = CreateClient();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Client?.Dispose();
        await _container.DisposeAsync();
    }
}
