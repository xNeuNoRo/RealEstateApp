using RealEstateApp.Api.Tests.Helpers;

namespace RealEstateApp.Api.Tests.Integration;

public sealed class PropertiesControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public PropertiesControllerTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task List_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/properties");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_AsDeveloper_Returns200WithProperties()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task List_AsDeveloper_ReturnsPropertyFields()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data");

        items.GetArrayLength().Should().BeGreaterThan(0);

        foreach (var item in items.EnumerateArray())
        {
            item.TryGetProperty("id", out _).Should().BeTrue();
            item.TryGetProperty("code", out _).Should().BeTrue();
            item.TryGetProperty("title", out _).Should().BeTrue();
            item.TryGetProperty("propertyType", out _).Should().BeTrue();
            item.TryGetProperty("saleType", out _).Should().BeTrue();
            item.TryGetProperty("price", out _).Should().BeTrue();
            item.TryGetProperty("landSize", out _).Should().BeTrue();
            item.TryGetProperty("bedrooms", out _).Should().BeTrue();
            item.TryGetProperty("bathrooms", out _).Should().BeTrue();
            item.TryGetProperty("agentName", out _).Should().BeTrue();
            item.TryGetProperty("status", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task List_AsAdmin_Returns200()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var response = await client.GetAsync("/api/v1/properties");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/properties/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_Existing_AsDeveloper_Returns200()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties/1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        var data = body.GetProperty("data");
        data.GetProperty("id").GetInt32().Should().Be(1);
        data.TryGetProperty("improvements", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_AsAdmin_Returns200()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var response = await client.GetAsync("/api/v1/properties/1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByCode_AsAnonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/properties/code/100001");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetByCode_Existing_AsDeveloper_Returns200()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties/code/100001");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("code").GetString().Should().Be("100001");
    }

    [Fact]
    public async Task GetByCode_NonExisting_Returns404()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties/code/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByCode_InvalidCode_Returns400()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties/code/ABC");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByCode_TooShort_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var response = await client.GetAsync("/api/v1/properties/code/12345");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByCode_TooLong_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var response = await client.GetAsync("/api/v1/properties/code/1234567");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task List_FilterByPropertyType_ValidId_ReturnsFilteredResults()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties?propertyTypeId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        foreach (var item in items)
            item.GetProperty("propertyType").GetString().Should().Be("Casa");
    }

    [Fact]
    public async Task List_FilterByPropertyType_InvalidId_ReturnsNoContent()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties?propertyTypeId=99999");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task List_FilterByPrice_MinOnly_ReturnsFilteredResults()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties?priceMin=5000000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        foreach (var item in items)
            item.GetProperty("price").GetDecimal().Should().BeGreaterThanOrEqualTo(5000000);
    }

    [Fact]
    public async Task List_FilterByPrice_MaxOnly_ReturnsFilteredResults()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties?priceMax=5000000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        foreach (var item in items)
            item.GetProperty("price").GetDecimal().Should().BeLessThanOrEqualTo(5000000);
    }

    [Fact]
    public async Task List_FilterByPrice_BothMinMax_ReturnsFilteredResults()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync(
            "/api/v1/properties?priceMin=3000000&priceMax=7000000"
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        foreach (var item in items)
        {
            var price = item.GetProperty("price").GetDecimal();
            price.Should().BeGreaterThanOrEqualTo(3000000);
            price.Should().BeLessThanOrEqualTo(7000000);
        }
    }

    [Fact]
    public async Task List_FilterByPrice_InvalidRange_MinGtMax_Returns400()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync(
            "/api/v1/properties?priceMin=10000000&priceMax=5000000"
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task List_FilterByBedrooms_ReturnsFilteredResults()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties?bedrooms=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        foreach (var item in items)
            item.GetProperty("bedrooms").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task List_FilterByBathrooms_ReturnsFilteredResults()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties?bathrooms=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        foreach (var item in items)
            item.GetProperty("bathrooms").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task List_FilterPagination_ReturnsCorrectPage()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties?page=1&pageSize=2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task List_FilterWithSearchTerm_ReturnsFilteredResults()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties?searchTerm=Casa");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        foreach (var item in items)
        {
            var title = item.GetProperty("title").GetString()!;
            title.Should().Contain("Casa");
        }
    }

    [Fact]
    public async Task List_FilterAllCombined_ReturnsFilteredResults()
    {
        await PropertySeeder.SeedAsync(_factory);
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync(
            "/api/v1/properties?propertyTypeId=1&priceMin=4000000&priceMax=10000000&bedrooms=3&bathrooms=2"
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var items = body.GetProperty("data").EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        foreach (var item in items)
        {
            item.GetProperty("propertyType").GetString().Should().Be("Casa");
            item.GetProperty("bedrooms").GetInt32().Should().Be(3);
            item.GetProperty("bathrooms").GetInt32().Should().Be(2);
            var price = item.GetProperty("price").GetDecimal();
            price.Should().BeGreaterThanOrEqualTo(4000000);
            price.Should().BeLessThanOrEqualTo(10000000);
        }
    }

    [Fact]
    public async Task List_WhenNoProperties_Returns204()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/properties");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
