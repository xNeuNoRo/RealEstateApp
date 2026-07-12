using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealEstateApp.Infrastructure.Persistence.Contexts;

/// <summary>
/// Factory para crear el contexto de Persistencia en tiempo de diseño (para migraciones).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("RealEstateDb")
            ?? throw new InvalidOperationException(
                $"Connection string 'RealEstateDb' no encontrada en la configuración (entorno: {env})."
            );

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
        );

        return new AppDbContext(
            optionsBuilder.Options,
            TimeProvider.System,
            NullLogger<AppDbContext>.Instance
        );
    }
}
