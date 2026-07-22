using RealEstateApp.Api.Tests.Helpers;

namespace RealEstateApp.Api.Tests.Integration;

public sealed class E2EIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public E2EIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task FullLifecycle_AdminCreatesDeveloper_NewDeveloperAccessesAPI()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uid = Guid.NewGuid().ToString("N")[..8];

        // Probamos como admin crear un nuevo developer
        var register = await admin.PostAsJsonAsync(
            "/api/v1/account/register/developer",
            new
            {
                firstName = $"E2E{uid}",
                lastName = $"Dev{uid}",
                identityDocument = ValidCedula(),
                email = $"e2edev{uid}@test.com",
                userName = $"e2edev{uid}",
                password = "E2eDevPass123!",
                confirmPassword = "E2eDevPass123!",
                role = "Developer",
            }
        );
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        // Probamos loguearnos como el nuevo developer y acceder a endpoints permitidos y bloqueados
        var devClient = _factory.CreateClient();
        var loginBody = await devClient.PostAsJsonAsync(
            "/api/v1/account/login",
            new { userNameOrEmail = $"e2edev{uid}", password = "E2eDevPass123!" }
        );
        loginBody.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginData = await loginBody.Content.ReadFromJsonAsync<JsonElement>(Json);
        var token = loginData.GetProperty("data").GetProperty("token").GetString()!;
        devClient.SetBearerToken(token);

        // El nuevo developer puede acceder a la lista de propiedades (read: allowed)
        var props = await devClient.GetAsync("/api/v1/properties");
        props.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // El nuevo developer puede acceder a la lista de agentes (read: allowed)
        var agents = await devClient.GetAsync("/api/v1/agents");
        agents.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // El nuevo developer no puede crear un nuevo PropertyType (write: blocked)
        var forbidden = await devClient.PostAsJsonAsync(
            "/api/v1/propertytype",
            new { name = $"E2E{uid}", description = "Should be forbidden." }
        );
        forbidden.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task FullLifecycle_CreatePropertyAndVerifyEndToEnd()
    {
        await PropertySeeder.SeedAsync(_factory);
        var dev = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var listResp = await dev.GetAsync("/api/v1/properties");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var listBody = await listResp.Content.ReadFromJsonAsync<JsonElement>(Json);
        var items = listBody.GetProperty("data").EnumerateArray().ToList();
        items.Should().NotBeEmpty();

        var firstId = items[0].GetProperty("id").GetInt32();
        var detailResp = await dev.GetAsync($"/api/v1/properties/{firstId}");
        detailResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var detailBody = await detailResp.Content.ReadFromJsonAsync<JsonElement>(Json);
        detailBody.GetProperty("data").GetProperty("id").GetInt32().Should().Be(firstId);

        var firstCode = items[0].GetProperty("code").GetString()!;
        var codeResp = await dev.GetAsync($"/api/v1/properties/code/{firstCode}");
        codeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var codeBody = await codeResp.Content.ReadFromJsonAsync<JsonElement>(Json);
        codeBody.GetProperty("data").GetProperty("code").GetString().Should().Be(firstCode);

        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var ptResp = await admin.GetAsync("/api/v1/propertytype");
        ptResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FullLifecycle_CatalogCascadeIntegrity()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var uid = Guid.NewGuid().ToString("N")[..8];
        var createPt = await admin.PostAsJsonAsync(
            "/api/v1/propertytype",
            new { name = $"E2EType-{uid}", description = "E2E test type." }
        );
        createPt.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdPt = await createPt.Content.ReadFromJsonAsync<JsonElement>(Json);
        var ptId = createdPt.GetProperty("data").GetProperty("id").GetInt32();
        var deletePt = await admin.DeleteAsync($"/api/v1/propertytype/{ptId}");
        deletePt.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getPt = await admin.GetAsync($"/api/v1/propertytype/{ptId}");
        getPt.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AuthorizationMatrix_AnonymousVsAdminVsDeveloper()
    {
        var anonymous = _factory.CreateClient();
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var dev = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var devEndpoints = new (string Method, string Path)[]
        {
            ("GET", "/api/v1/properties"),
            ("GET", "/api/v1/properties/1"),
            ("GET", "/api/v1/properties/code/100001"),
            ("GET", "/api/v1/agents"),
            ("GET", "/api/v1/agents/some-id"),
            ("GET", "/api/v1/agents/some-id/properties"),
            ("GET", "/api/v1/propertytype/1"),
            ("GET", "/api/v1/saletype/1"),
            ("GET", "/api/v1/improvement/1"),
        };
        var devForbiddenEndpoints = new (string Method, string Path)[]
        {
            ("GET", "/api/v1/propertytype"),
            ("GET", "/api/v1/saletype"),
            ("GET", "/api/v1/improvement"),
        };

        foreach (var (method, path) in devEndpoints)
        {
            var anonResp = await anonymous.GetAsync(path);
            anonResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var resp = await dev.GetAsync(path);
            resp.StatusCode.Should()
                .BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        foreach (var (method, path) in devForbiddenEndpoints)
        {
            var resp = await dev.GetAsync(path);
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    public async Task AuthorizationMatrix_AdminOnlyEndpoints()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var dev = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var adminOnly = new (string Method, string Path, object? Body)[]
        {
            ("POST", "/api/v1/propertytype", new { name = "E2EAdminOnly", description = "Test." }),
            ("PUT", "/api/v1/propertytype/1", new { name = "Updated", description = "Test." }),
            ("DELETE", "/api/v1/propertytype/99999", null),
            (
                "POST",
                "/api/v1/saletype",
                new
                {
                    code = "Sale",
                    name = "E2E",
                    description = "Test.",
                }
            ),
            (
                "PUT",
                "/api/v1/saletype/1",
                new
                {
                    id = 1,
                    code = "Sale",
                    name = "Updated",
                    description = "Test.",
                }
            ),
            ("DELETE", "/api/v1/saletype/99999", null),
            ("POST", "/api/v1/improvement", new { name = "E2EImp", description = "Test." }),
            ("PUT", "/api/v1/improvement/1", new { name = "Updated", description = "Test." }),
            ("DELETE", "/api/v1/improvement/99999", null),
            ("PATCH", "/api/v1/agents/some-id/toggle", null),
            ("PATCH", "/api/v1/agents/some-id/status", new { status = true }),
            (
                "POST",
                "/api/v1/account/register/developer",
                new
                {
                    firstName = "E2E",
                    lastName = "Forbidden",
                    identityDocument = "00000000000",
                    email = "fail@test.com",
                    userName = "e2e_forbidden",
                    password = "Fail123!",
                    confirmPassword = "Fail123!",
                    role = "Developer",
                }
            ),
        };

        foreach (var (method, path, body) in adminOnly)
        {
            var devResp = await ExecuteAsync(dev, method, path, body);
            devResp
                .StatusCode.Should()
                .Be(HttpStatusCode.Forbidden, $"Developer should get 403 for {method} {path}");

            var adminResp = await ExecuteAsync(admin, method, path, body);
            adminResp
                .StatusCode.Should()
                .NotBe(HttpStatusCode.Forbidden, $"Admin should not get 403 for {method} {path}");
        }
    }

    [Fact]
    public async Task ErrorHandling_MalformedRequests()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var invalidId = await client.GetAsync("/api/v1/properties/abc");
        invalidId.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var invalidCode = await client.GetAsync("/api/v1/properties/code/12");
        invalidCode.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var missingFields = await admin.PostAsJsonAsync("/api/v1/propertytype", new { });
        missingFields.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ErrorHandling_ExpiredOrInvalidToken()
    {
        var client = _factory.CreateClient();
        client.SetBearerToken(
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.invalid"
        );

        var response = await client.GetAsync("/api/v1/properties");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ErrorHandling_NonExistentEndpoints()
    {
        var client = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");

        var response = await client.GetAsync("/api/v1/nonexistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullLifecycle_CatalogPropertyTypeCRUD()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uid = Guid.NewGuid().ToString("N")[..8];
        var name = $"E2EPT-{uid}";
        var updatedName = $"{name}_Upd";

        var create = await admin.PostAsJsonAsync(
            "/api/v1/propertytype",
            new { name, description = "E2E test type." }
        );
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>(Json);
        var ptId = created.GetProperty("data").GetProperty("id").GetInt32();
        created.GetProperty("data").GetProperty("name").GetString().Should().Be(name);

        var get = await admin.GetAsync($"/api/v1/propertytype/{ptId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var got = await get.Content.ReadFromJsonAsync<JsonElement>(Json);
        got.GetProperty("data").GetProperty("name").GetString().Should().Be(name);

        var update = await admin.PutAsJsonAsync(
            $"/api/v1/propertytype/{ptId}",
            new { name = updatedName, description = "Updated description." }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var getUpdated = await admin.GetAsync($"/api/v1/propertytype/{ptId}");
        getUpdated.StatusCode.Should().Be(HttpStatusCode.OK);
        var gotUpdated = await getUpdated.Content.ReadFromJsonAsync<JsonElement>(Json);
        gotUpdated.GetProperty("data").GetProperty("name").GetString().Should().Be(updatedName);

        var delete = await admin.DeleteAsync($"/api/v1/propertytype/{ptId}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeleted = await admin.GetAsync($"/api/v1/propertytype/{ptId}");
        getDeleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullLifecycle_CatalogSaleTypeCRUD()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var deleteSeed = await admin.DeleteAsync("/api/v1/saletype/3");
        deleteSeed.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);

        var uid = Guid.NewGuid().ToString("N")[..8];

        var create = await admin.PostAsJsonAsync(
            "/api/v1/saletype",
            new
            {
                code = "RentToOwn",
                name = $"E2EST-{uid}",
                description = "E2E test sale type.",
            }
        );
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>(Json);
        var stId = created.GetProperty("data").GetProperty("id").GetInt32();
        created.GetProperty("data").GetProperty("code").GetString().Should().Be("RentToOwn");

        var get = await admin.GetAsync($"/api/v1/saletype/{stId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var update = await admin.PutAsJsonAsync(
            $"/api/v1/saletype/{stId}",
            new
            {
                id = stId,
                code = "RentToOwn",
                name = $"E2EST-{uid}_Upd",
                description = "Updated.",
            }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var delete = await admin.DeleteAsync($"/api/v1/saletype/{stId}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeleted = await admin.GetAsync($"/api/v1/saletype/{stId}");
        getDeleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullLifecycle_CatalogImprovementCRUD()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var uid = Guid.NewGuid().ToString("N")[..8];
        var name = $"E2EImp-{uid}";
        var updatedName = $"{name}_Upd";

        var create = await admin.PostAsJsonAsync(
            "/api/v1/improvement",
            new { name, description = "E2E test improvement." }
        );
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>(Json);
        var impId = created.GetProperty("data").GetProperty("id").GetInt32();
        created.GetProperty("data").GetProperty("name").GetString().Should().Be(name);

        var get = await admin.GetAsync($"/api/v1/improvement/{impId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var got = await get.Content.ReadFromJsonAsync<JsonElement>(Json);
        got.GetProperty("data").GetProperty("name").GetString().Should().Be(name);

        var update = await admin.PutAsJsonAsync(
            $"/api/v1/improvement/{impId}",
            new { name = updatedName, description = "Updated." }
        );
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var getUpdated = await admin.GetAsync($"/api/v1/improvement/{impId}");
        getUpdated.StatusCode.Should().Be(HttpStatusCode.OK);
        var gotUpdated = await getUpdated.Content.ReadFromJsonAsync<JsonElement>(Json);
        gotUpdated.GetProperty("data").GetProperty("name").GetString().Should().Be(updatedName);

        var delete = await admin.DeleteAsync($"/api/v1/improvement/{impId}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeleted = await admin.GetAsync($"/api/v1/improvement/{impId}");
        getDeleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullLifecycle_AgentToggleAndStatusFlow()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");

        var listResp = await admin.GetAsync("/api/v1/agents");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var listBody = await listResp.Content.ReadFromJsonAsync<JsonElement>(Json);
        var agents = listBody.GetProperty("data").EnumerateArray().ToList();
        agents.Should().NotBeEmpty();

        var agent = agents.First();
        var agentId = agent.GetProperty("id").GetString()!;
        var originalStatus = agent.GetProperty("status").GetBoolean();

        var toggle = await admin.PatchAsync($"/api/v1/agents/{agentId}/toggle", null);
        toggle.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listAfterToggle = await admin.GetAsync("/api/v1/agents");
        var listAfterToggleBody = await listAfterToggle.Content.ReadFromJsonAsync<JsonElement>(
            Json
        );
        var toggledAgent = listAfterToggleBody
            .GetProperty("data")
            .EnumerateArray()
            .First(a => a.GetProperty("id").GetString() == agentId);
        toggledAgent.GetProperty("status").GetBoolean().Should().Be(!originalStatus);

        var restore = await admin.PatchAsJsonAsync(
            $"/api/v1/agents/{agentId}/status",
            new { status = originalStatus }
        );
        restore.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listAfterRestore = await admin.GetAsync("/api/v1/agents");
        var listAfterRestoreBody = await listAfterRestore.Content.ReadFromJsonAsync<JsonElement>(
            Json
        );
        var restoredAgent = listAfterRestoreBody
            .GetProperty("data")
            .EnumerateArray()
            .First(a => a.GetProperty("id").GetString() == agentId);
        restoredAgent.GetProperty("status").GetBoolean().Should().Be(originalStatus);
    }

    [Fact]
    public async Task Security_CrossRoleCatalogAccess()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var dev = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var uid = Guid.NewGuid().ToString("N")[..8];
        var name = $"E2ECross-{uid}";

        var create = await admin.PostAsJsonAsync(
            "/api/v1/propertytype",
            new { name, description = "Cross-role test." }
        );
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>(Json);
        var ptId = created.GetProperty("data").GetProperty("id").GetInt32();

        var devRead = await dev.GetAsync($"/api/v1/propertytype/{ptId}");
        devRead.StatusCode.Should().Be(HttpStatusCode.OK);

        var devUpdate = await dev.PutAsJsonAsync(
            $"/api/v1/propertytype/{ptId}",
            new { name = "Hacked", description = "Should be forbidden." }
        );
        devUpdate.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var devDelete = await dev.DeleteAsync($"/api/v1/propertytype/{ptId}");
        devDelete.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var adminUpdate = await admin.PutAsJsonAsync(
            $"/api/v1/propertytype/{ptId}",
            new { name = $"{name}_AdminUpd", description = "Admin updated." }
        );
        adminUpdate.StatusCode.Should().Be(HttpStatusCode.OK);

        var adminRead = await admin.GetAsync($"/api/v1/propertytype/{ptId}");
        adminRead.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminReadBody = await adminRead.Content.ReadFromJsonAsync<JsonElement>(Json);
        adminReadBody
            .GetProperty("data")
            .GetProperty("name")
            .GetString()
            .Should()
            .Be($"{name}_AdminUpd");

        var adminDelete = await admin.DeleteAsync($"/api/v1/propertytype/{ptId}");
        adminDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FullLifecycle_MultiCatalogConsistency()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
        var dev = await _factory.CreateAuthenticatedClientAsync("developer", "Developer123!");
        var uid = Guid.NewGuid().ToString("N")[..8];
        var ptName = $"E2EMultiPT-{uid}";
        var stName = $"E2EMultiST-{uid}";
        var impName = $"E2EMultiImp-{uid}";

        var freeCode = await admin.DeleteAsync("/api/v1/saletype/3");
        freeCode.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);

        var createPt = await admin.PostAsJsonAsync(
            "/api/v1/propertytype",
            new { name = ptName, description = "Multi PT." }
        );
        createPt.StatusCode.Should().Be(HttpStatusCode.Created);
        var ptJson = await createPt.Content.ReadFromJsonAsync<JsonElement>(Json);
        var ptId = ptJson.GetProperty("data").GetProperty("id").GetInt32();

        var createSt = await admin.PostAsJsonAsync(
            "/api/v1/saletype",
            new
            {
                code = "RentToOwn",
                name = stName,
                description = "Multi ST.",
            }
        );
        createSt.StatusCode.Should().Be(HttpStatusCode.Created);
        var stJson = await createSt.Content.ReadFromJsonAsync<JsonElement>(Json);
        var stId = stJson.GetProperty("data").GetProperty("id").GetInt32();

        var createImp = await admin.PostAsJsonAsync(
            "/api/v1/improvement",
            new { name = impName, description = "Multi Imp." }
        );
        createImp.StatusCode.Should().Be(HttpStatusCode.Created);
        var impJson = await createImp.Content.ReadFromJsonAsync<JsonElement>(Json);
        var impId = impJson.GetProperty("data").GetProperty("id").GetInt32();

        var devPt = await dev.GetAsync($"/api/v1/propertytype/{ptId}");
        devPt.StatusCode.Should().Be(HttpStatusCode.OK);

        var devSt = await dev.GetAsync($"/api/v1/saletype/{stId}");
        devSt.StatusCode.Should().Be(HttpStatusCode.OK);

        var devImp = await dev.GetAsync($"/api/v1/improvement/{impId}");
        devImp.StatusCode.Should().Be(HttpStatusCode.OK);

        var devPtList = await dev.GetAsync("/api/v1/propertytype");
        devPtList.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var devStList = await dev.GetAsync("/api/v1/saletype");
        devStList.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var devImpList = await dev.GetAsync("/api/v1/improvement");
        devImpList.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var adminPtList = await admin.GetAsync("/api/v1/propertytype");
        adminPtList.StatusCode.Should().Be(HttpStatusCode.OK);

        var adminStList = await admin.GetAsync("/api/v1/saletype");
        adminStList.StatusCode.Should().Be(HttpStatusCode.OK);

        var adminImpList = await admin.GetAsync("/api/v1/improvement");
        adminImpList.StatusCode.Should().Be(HttpStatusCode.OK);

        var delPt = await admin.DeleteAsync($"/api/v1/propertytype/{ptId}");
        delPt.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var delSt = await admin.DeleteAsync($"/api/v1/saletype/{stId}");
        delSt.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var delImp = await admin.DeleteAsync($"/api/v1/improvement/{impId}");
        delImp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<HttpResponseMessage> ExecuteAsync(
        HttpClient client,
        string method,
        string path,
        object? body
    )
    {
        return method switch
        {
            "GET" => await client.GetAsync(path),
            "POST" => await client.PostAsJsonAsync(path, body!),
            "PUT" => await client.PutAsJsonAsync(path, body!),
            "DELETE" => await client.DeleteAsync(path),
            "PATCH" when body is not null => await client.PatchAsJsonAsync(path, body),
            "PATCH" => await client.PatchAsync(path, null),
            _ => throw new InvalidOperationException(),
        };
    }

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
}
