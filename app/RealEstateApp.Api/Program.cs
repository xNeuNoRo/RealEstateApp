using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Infrastructure.Identity;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddIdentityForWebApi(builder.Configuration);

var app = builder.Build();

await app.Services.RunIdentitySeedAsync();

await app.RunAsync();
