using RealEstateApp.Api.Tests.Helpers;

namespace RealEstateApp.Api.Tests.Integration;

public class SmokeTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public SmokeTests(ApiFactory factory)
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
    public async Task Login_WithInvalidCredentials_ReturnsError()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/account/login",
            new { userNameOrEmail = "nonexistent", password = "wrong" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PropertiesList_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/properties");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PropertiesList_WithDeveloperToken_Returns200Or204()
    {
        var client = _factory.CreateClient();
        var token = await client.LoginAsDeveloperAsync();
        client.SetBearerToken(token);

        var response = await client.GetAsync("/api/v1/properties");

        response.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.NoContent]);
    }

    [Fact]
    public async Task AgentsList_WithAdminToken_Returns200Or204()
    {
        var client = _factory.CreateClient();
        var token = await client.LoginAsAdminAsync();
        client.SetBearerToken(token);

        var response = await client.GetAsync("/api/v1/agents");

        response.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.NoContent]);
    }
}
