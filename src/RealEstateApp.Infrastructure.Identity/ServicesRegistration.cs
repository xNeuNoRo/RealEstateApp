using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Admin;
using RealEstateApp.Application.Interfaces.UseCases.Agent;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Application.Interfaces.UseCases.Client;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Infrastructure.Identity.Contexts;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Repositories;
using RealEstateApp.Infrastructure.Identity.Seeds;
using RealEstateApp.Infrastructure.Identity.Services;
using RealEstateApp.Infrastructure.Identity.UseCases.Admin;
using RealEstateApp.Infrastructure.Identity.UseCases.Agent;
using RealEstateApp.Infrastructure.Identity.UseCases.Auth;
using RealEstateApp.Infrastructure.Identity.UseCases.Client;

namespace RealEstateApp.Infrastructure.Identity;

public static class ServicesRegistration
{
    /// <summary>
    /// Configura Identity con JWT para la WebApi.
    /// </summary>
    public static IServiceCollection AddIdentityForWebApi(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddIdentityCommon(configuration);

        // --- JWT ---
        var jwtSettings =
            configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "JwtSettings no está configurado correctamente en appsettings.Development.json"
            );

        services
            .AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = false;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                    ),
                };
                opt.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var payload = JsonSerializer.Serialize(
                            new { error = "No está autorizado para acceder a este recurso." }
                        );
                        return context.Response.WriteAsync(payload);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var payload = JsonSerializer.Serialize(
                            new
                            {
                                error = "Acceso denegado. No tiene permisos para realizar esta acción.",
                            }
                        );
                        return context.Response.WriteAsync(payload);
                    },
                };
            });

        services.AddAuthorization();

        // Servicio legado exclusivo de la API JWT.
        services.AddScoped<IAccountServiceForWebApi, AccountServiceForWebApi>();

        return services;
    }

    /// <summary>
    /// Registra la infraestructura común de Identity (DbContext, Identity Core, servicios, Use Cases).
    /// Compartido entre WebApi y WebApp.
    /// </summary>
    internal static IServiceCollection AddIdentityCommon(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // --- DbContext ---
        services.AddDbContext<IdentityContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("RealEstateDb"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                    sql.CommandTimeout(30);
                }
            )
        );

        // --- JWT Settings ---
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // --- Identity ---
        services
            .AddIdentityCore<AppUser>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        services.AddAuthorization();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddAutoMapper(cfg => { }, typeof(ServicesRegistration).Assembly);

        return services;
    }

    /// <summary>
    /// Configura Identity con Cookie auth para la WebApp (MVC/Razor Pages).
    /// </summary>
    public static IServiceCollection AddIdentityForWebApp(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddIdentityCommon(configuration);
        services.AddScoped<IAccountServiceForWebApp, AccountServiceForWebApp>();
        services.AddAllIdentityUseCases();

        services
            .AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(opt =>
        {
            opt.LoginPath = "/Auth/Login";
            opt.AccessDeniedPath = "/Auth/AccessDenied";
            opt.SlidingExpiration = true;
            opt.ExpireTimeSpan = TimeSpan.FromDays(7);
            opt.Cookie.HttpOnly = true;
            opt.Cookie.SameSite = SameSiteMode.Lax;
            opt.Cookie.Name = ".RealEstateApp.Auth";
        });

        return services;
    }

    internal static IServiceCollection AddAllIdentityUseCases(this IServiceCollection services)
    {
        services.AddAuthUseCases();
        services.AddAdminUseCases();
        services.AddIdentityClientUseCases();
        services.AddIdentityAgentUseCases();

        return services;
    }

    /// <summary>
    /// Registra los Use Cases de autenticación para la WebApp (cookies, no JWT).
    /// </summary>
    internal static IServiceCollection AddAuthUseCases(this IServiceCollection services)
    {
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IRegisterClientUseCase, RegisterClientUseCase>();
        services.AddScoped<IRegisterAgentUseCase, RegisterAgentUseCase>();
        services.AddScoped<IActivateAccountUseCase, ActivateAccountUseCase>();
        services.AddScoped<IResendActivationUseCase, ResendActivationUseCase>();
        services.AddScoped<IForgotPasswordUseCase, ForgotPasswordUseCase>();
        services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
        services.AddScoped<IChangePasswordUseCase, ChangePasswordUseCase>();

        return services;
    }

    /// <summary>
    /// Registra los Use Cases del módulo Admin
    /// </summary>
    internal static IServiceCollection AddAdminUseCases(this IServiceCollection services)
    {
        services.AddScoped<IGetAdminDashboardUseCase, GetAdminDashboardUseCase>();
        services.AddScoped<IGetAgentsListUseCase, GetAgentsListUseCase>();
        services.AddScoped<IGetAgentByIdUseCase, GetAgentByIdUseCase>();
        services.AddScoped<IToggleAgentActiveUseCase, ToggleAgentActiveUseCase>();
        services.AddScoped<IChangeAgentStatusUseCase, ChangeAgentStatusUseCase>();
        services.AddScoped<IDeleteAgentUseCase, DeleteAgentUseCase>();
        services.AddScoped<IGetAdminsListUseCase, GetAdminsListUseCase>();
        services.AddScoped<ICreateAdminUseCase, CreateAdminUseCase>();
        services.AddScoped<IUpdateAdminUseCase, UpdateAdminUseCase>();
        services.AddScoped<IToggleAdminActiveUseCase, ToggleAdminActiveUseCase>();
        services.AddScoped<IGetDevelopersListUseCase, GetDevelopersListUseCase>();
        services.AddScoped<ICreateDeveloperUseCase, CreateDeveloperUseCase>();
        services.AddScoped<IUpdateDeveloperUseCase, UpdateDeveloperUseCase>();
        services.AddScoped<IToggleDeveloperActiveUseCase, ToggleDeveloperActiveUseCase>();

        return services;
    }

    /// <summary>
    /// Registra los Use Cases de cliente que requieren UserManager (Identity).
    /// </summary>
    internal static IServiceCollection AddIdentityClientUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUpdateClientProfileUseCase, UpdateClientProfileUseCase>();

        return services;
    }

    internal static IServiceCollection AddIdentityAgentUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUpdateAgentProfileUseCase, UpdateAgentProfileUseCase>();

        return services;
    }

    /// <summary>
    /// Ejecuta los seeds de roles y usuarios por defecto.
    /// </summary>
    public static async Task RunIdentitySeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<AppUser>>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var loggerFactory = provider.GetService<ILoggerFactory>();

        await DefaultRoles.SeedAsync(roleManager);
        await DefaultAdminUser.SeedAsync(
            userManager,
            configuration,
            loggerFactory?.CreateLogger("DefaultAdminUser")
        );
        await DefaultDeveloperUser.SeedAsync(
            userManager,
            configuration,
            loggerFactory?.CreateLogger("DefaultDeveloperUser")
        );
        await DefaultClientUser.SeedAsync(
            userManager,
            configuration,
            loggerFactory?.CreateLogger("DefaultClientUser")
        );
        await DefaultAgentUser.SeedAsync(
            userManager,
            configuration,
            loggerFactory?.CreateLogger("DefaultAgentUser")
        );
    }
}
