using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Api.Tests.Helpers;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Api.Tests.Integration;

public sealed class AgentsControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public AgentsControllerTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetAgentIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var agent = await userManager.FindByNameAsync("agente");
        if (agent is null)
            throw new InvalidOperationException(
                "El agente 'agente' no existe. Asegúrate de que el seeder de datos se haya ejecutado correctamente."
            );
        var roles = await userManager.GetRolesAsync(agent);
        if (!roles.Contains("Agent"))
            throw new InvalidOperationException(
                $"El agente 'agente' tiene los siguientes roles: [{string.Join(", ", roles)}]"
            );
        return agent.Id;
    }

    [Fact]
    public async Task List_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/agents");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_AsDeveloper_Returns200()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var response = await client.GetAsync("/api/v1/agents");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task List_AsDeveloper_ReturnsRequiredFields()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var response = await client.GetAsync("/api/v1/agents");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data");

        foreach (var item in items.EnumerateArray())
        {
            item.TryGetProperty("id", out _).Should().BeTrue();
            item.TryGetProperty("name", out _).Should().BeTrue();
            item.TryGetProperty("lastName", out _).Should().BeTrue();
            item.TryGetProperty("propertiesCount", out _).Should().BeTrue();
            item.TryGetProperty("email", out _).Should().BeTrue();
            item.TryGetProperty("phone", out _).Should().BeTrue();
            item.TryGetProperty("status", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task List_AsAdmin_Returns200()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var response = await client.GetAsync("/api/v1/agents");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ExistingAgent_AsDeveloper_Returns200()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync($"/api/v1/agents/{agentId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ExistingAgent_AsDeveloper_ReturnsAllFields()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync($"/api/v1/agents/{agentId}");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var data = body.GetProperty("data");

        data.GetProperty("id").GetString().Should().Be(agentId);
        data.TryGetProperty("name", out _).Should().BeTrue();
        data.TryGetProperty("lastName", out _).Should().BeTrue();
        data.TryGetProperty("propertiesCount", out _).Should().BeTrue();
        data.TryGetProperty("email", out _).Should().BeTrue();
        data.TryGetProperty("phone", out _).Should().BeTrue();
        data.TryGetProperty("status", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var response = await client.GetAsync("/api/v1/agents/nonexistent-id-12345");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/agents/some-id");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAgentProperties_ExistingAgent_AsDeveloper_Returns200()
    {
        await PropertySeeder.SeedAsync(_factory);
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync($"/api/v1/agents/{agentId}/properties");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAgentProperties_NonExistingAgent_Returns404()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var response = await client.GetAsync("/api/v1/agents/nonexistent-id-12345/properties");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAgentProperties_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/agents/some-id/properties");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ToggleStatus_ExistingAgent_AsAdmin_Returns204()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var response = await client.PatchAsync($"/api/v1/agents/{agentId}/toggle", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ToggleStatus_NonExistingAgent_Returns404()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var response = await client.PatchAsync("/api/v1/agents/nonexistent-id-12345/toggle", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleStatus_AsDeveloper_Returns403()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.PatchAsync($"/api/v1/agents/{agentId}/toggle", null);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ToggleStatus_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PatchAsync("/api/v1/agents/some-id/toggle", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeStatus_ExistingAgent_AsAdmin_Returns204()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/agents/{agentId}/status",
            new { status = false }
        );
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangeStatus_NonExistingAgent_Returns404()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var response = await client.PatchAsJsonAsync(
            "/api/v1/agents/nonexistent-id-12345/status",
            new { status = true }
        );
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeStatus_AsDeveloper_Returns403()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/agents/{agentId}/status",
            new { status = true }
        );
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangeStatus_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PatchAsJsonAsync(
            "/api/v1/agents/some-id/status",
            new { status = true }
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeStatus_InvalidBody_Returns400()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var response = await client.PatchAsync(
            $"/api/v1/agents/{agentId}/status",
            new StringContent("not-json", Encoding.UTF8, "application/json")
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_DeveloperIdNotAgent_Returns404()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var developer =
            await userManager.FindByNameAsync("developer")
            ?? throw new InvalidOperationException("Seed user 'developer' not found.");
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync($"/api/v1/agents/{developer.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAgentProperties_AgentWithNoProperties_Returns204()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync($"/api/v1/agents/{agentId}/properties");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangeStatus_InvalidStatusValue_Returns400()
    {
        var agentId = await GetAgentIdAsync();
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/agents/{agentId}/status",
            new { status = "not-a-boolean" }
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
