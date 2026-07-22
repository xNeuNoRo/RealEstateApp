using RealEstateApp.Api.Tests.Helpers;

namespace RealEstateApp.Api.Tests.Integration;

public sealed class AccountControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>Genera una cedula valida</summary>
    private static string ValidCedula()
    {
        var rng = Random.Shared;
        var digits = new int[11];
        for (int i = 0; i < 10; i++)
            digits[i] = rng.Next(10);
        int sum = 0;
        for (int i = 0; i < 10; i++)
        {
            int prod = digits[i] * ((i % 2 == 0) ? 1 : 2);
            sum += prod >= 10 ? prod - 9 : prod;
        }
        digits[10] = (10 - (sum % 10)) % 10;
        return string.Concat(digits);
    }

    public AccountControllerTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_AsDeveloper_ReturnsToken()
    {
        var client = _factory.CreateClient();
        var token = await client.LoginAsDeveloperAsync();
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_AsAdmin_ReturnsToken()
    {
        var client = _factory.CreateClient();
        var token = await client.LoginAsAdminAsync();
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/login",
            new { userNameOrEmail = "developer", password = "WrongPassword123!" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeFalse();
        body.GetProperty("error")
            .GetProperty("code")
            .GetString()
            .Should()
            .Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Login_WithNonexistentUser_Returns400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/login",
            new { userNameOrEmail = "nonexistent_user_12345", password = "SomePass123!" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_Returns400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/login",
            new { userNameOrEmail = "", password = "" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithoutBody_Returns415()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync(
            "/api/v1/account/login",
            new StringContent("{}", new MediaTypeHeaderValue("application/json"))
        );

        // El endpoint espera un body con las propiedades userNameOrEmail y password,
        // Asi que el modelbinding dando error deberia tirar un 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterDeveloper_AsAdmin_Returns201()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"Test{uniqueId}",
                lastName = $"Dev{uniqueId}",
                identityDocument = ValidCedula(),
                email = $"dev{uniqueId}@test.com",
                userName = $"dev{uniqueId}",
                password = "TestDev123!",
                confirmPassword = "TestDev123!",
                role = "Developer",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("userName").GetString().Should().Be($"dev{uniqueId}");
    }

    [Fact]
    public async Task RegisterDeveloper_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = "Test",
                lastName = "Dev",
                identityDocument = ValidCedula(),
                email = "anondev_dup@test.com",
                userName = "anondev_dup",
                password = "TestAnon123!",
                confirmPassword = "TestAnon123!",
                role = "Developer",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterDeveloper_AsDeveloper_Returns403()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = "Test",
                lastName = "Dev",
                identityDocument = ValidCedula(),
                email = $"devdev{uniqueId}@test.com",
                userName = $"devdev{uniqueId}",
                password = "TestDev123!",
                confirmPassword = "TestDev123!",
                role = "Developer",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegisterDeveloper_WithDuplicateEmail_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var baseId = Guid.NewGuid().ToString("N")[..8];

        // Primero creamos un usuario
        var cedula1 = ValidCedula();
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"First{baseId}",
                lastName = $"Dev{baseId}",
                identityDocument = cedula1,
                email = $"first{baseId}@test.com",
                userName = $"first{baseId}",
                password = "TestDev123!",
                confirmPassword = "TestDev123!",
                role = "Developer",
            }
        );
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Luego intentamos registrar otro usuario con el mismo email
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"Second{baseId}",
                lastName = $"Dev{baseId}",
                identityDocument = ValidCedula(),
                email = $"first{baseId}@test.com",
                userName = $"second{baseId}",
                password = "TestDev123!",
                confirmPassword = "TestDev123!",
                role = "Developer",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterDeveloper_WithWeakPassword_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"Weak{uniqueId}",
                lastName = $"Dev{uniqueId}",
                identityDocument = ValidCedula(),
                email = $"weak{uniqueId}@test.com",
                userName = $"weakdev{uniqueId}",
                password = "123",
                confirmPassword = "123",
                role = "Developer",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterAdmin_AsAdmin_Returns201()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/admin",
            new
            {
                firstName = $"Test{uniqueId}",
                lastName = $"Admin{uniqueId}",
                identityDocument = ValidCedula(),
                email = $"admin{uniqueId}@test.com",
                userName = $"admin{uniqueId}",
                password = "TestAdmin123!",
                confirmPassword = "TestAdmin123!",
                role = "Admin",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterAdmin_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/admin",
            new
            {
                firstName = "Test",
                lastName = "Admin",
                identityDocument = ValidCedula(),
                email = $"anonadmin{uniqueId}@test.com",
                userName = $"anonadmin{uniqueId}",
                password = "TestAdmin123!",
                confirmPassword = "TestAdmin123!",
                role = "Admin",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterAdmin_AsDeveloper_Returns403()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/admin",
            new
            {
                firstName = "Test",
                lastName = "Admin",
                identityDocument = ValidCedula(),
                email = $"devadmin{uniqueId}@test.com",
                userName = $"devadmin{uniqueId}",
                password = "TestAdmin123!",
                confirmPassword = "TestAdmin123!",
                role = "Admin",
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_InactiveUser_Returns400()
    {
        await UserSeeder.SeedAuthTestUsersAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/account/login",
            new { userNameOrEmail = "inactive_dev", password = "InactiveDev1!" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Login_ClientRole_Returns400()
    {
        await UserSeeder.SeedAuthTestUsersAsync(_factory);
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/account/login",
            new { userNameOrEmail = "test_client", password = "TestClient1!" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Login_AgentRole_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/account/login",
            new { userNameOrEmail = "agente", password = "Agente123!" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task RegisterDeveloper_DuplicateCedula_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var cedula = ValidCedula();

        // Primer registro con cedula tendria q tirar 201
        var first = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"First{uniqueId}",
                lastName = "Dev",
                identityDocument = cedula,
                email = $"first{uniqueId}@test.com",
                userName = $"first{uniqueId}",
                password = "TestDev123!",
                confirmPassword = "TestDev123!",
                role = "Developer",
            }
        );
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        // Segundo registro con misma cedula deberia tirar 400
        var second = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"Second{uniqueId}",
                lastName = "Dev",
                identityDocument = cedula,
                email = $"second{uniqueId}@test.com",
                userName = $"second{uniqueId}",
                password = "TestDev123!",
                confirmPassword = "TestDev123!",
                role = "Developer",
            }
        );
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterDeveloper_InvalidCedulaFormat_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"Test{uniqueId}",
                lastName = "Dev",
                identityDocument = "12",
                email = $"test{uniqueId}@test.com",
                userName = $"test{uniqueId}",
                password = "TestDev123!",
                confirmPassword = "TestDev123!",
                role = "Developer",
            }
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterDeveloper_InvalidEmailFormat_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"Test{uniqueId}",
                lastName = "Dev",
                identityDocument = ValidCedula(),
                email = "not-an-email",
                userName = $"test{uniqueId}",
                password = "TestDev123!",
                confirmPassword = "TestDev123!",
                role = "Developer",
            }
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterDeveloper_PasswordMismatch_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"Test{uniqueId}",
                lastName = "Dev",
                identityDocument = ValidCedula(),
                email = $"test{uniqueId}@test.com",
                userName = $"test{uniqueId}",
                password = "TestDev123!",
                confirmPassword = "DifferentPass123!",
                role = "Developer",
            }
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
