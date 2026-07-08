using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RealEstateApp.Infrastructure.Identity.Contexts;

/// <summary>
/// Factory para crear el contexto de Identity en tiempo de diseño (para migraciones).
/// </summary>
public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("RealEstateDb")
            ?? throw new InvalidOperationException(
                "Connection string 'RealEstateDb' no encontrada en appsettings.Development.json"
            );

        var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)
        );

        return new IdentityContext(optionsBuilder.Options);
    }
}
