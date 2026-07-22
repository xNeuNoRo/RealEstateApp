using RealEstateApp.Application.Dtos.Auth;

namespace RealEstateApp.Api.Tests.Helpers;

public static class AuthHelper
{
    private const string LoginPath = "/api/v1/account/login";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task<string> LoginAsDeveloperAsync(this HttpClient client)
    {
        return await LoginAsync(client, "developer", "Developer123!");
    }

    public static async Task<string> LoginAsAdminAsync(this HttpClient client)
    {
        return await LoginAsync(client, "admin", "Admin123!");
    }

    public static async Task<string> LoginAsync(
        this HttpClient client,
        string userNameOrEmail,
        string password
    )
    {
        var response = await client.PostAsJsonAsync(
            LoginPath,
            new LoginDto { UserNameOrEmail = userNameOrEmail, Password = password }
        );

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseWrapper>(JsonOptions);
        return body?.Data?.Token
            ?? throw new InvalidOperationException(
                $"Error al iniciar sesión para '{userNameOrEmail}': no se encontró el token en la respuesta."
            );
    }

    public static void SetBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        this ApiFactory factory,
        string userNameOrEmail,
        string password
    )
    {
        var client = factory.CreateClient();
        var token = await client.LoginAsync(userNameOrEmail, password);
        client.SetBearerToken(token);
        return client;
    }

    private sealed class LoginResponseWrapper
    {
        public LoginResponseDto? Data { get; set; }
        public bool Success { get; set; }
    }
}
