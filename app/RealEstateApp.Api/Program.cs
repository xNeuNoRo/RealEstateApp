using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Application;
using RealEstateApp.Infrastructure.Identity;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddIdentityForWebApi(builder.Configuration);

builder.Services.AddPropertyUseCases();
builder.Services.AddOfferUseCases();
builder.Services.AddFavoriteUseCases();
builder.Services.AddChatUseCases();
builder.Services.AddCatalogUseCases();
builder.Services.AddClientUseCases();
builder.Services.AddAuthUseCases();
builder.Services.AddAdminUseCases();
builder.Services.AddIdentityClientUseCases();

var app = builder.Build();

await app.Services.RunIdentitySeedAsync();

await app.RunAsync();
