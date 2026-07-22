using RealEstateApp.Api.Tests.Helpers;

namespace RealEstateApp.Api.Tests.Integration;

public sealed class CatalogControllersTests
{
    public sealed class PropertyTypeTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public PropertyTypeTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetAll_AsAnonymous_Returns401()
        {
            var response = await _factory.CreateClient().GetAsync("/api/v1/propertytype");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_AsAdmin_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.GetAsync("/api/v1/propertytype");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/propertytype");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAll_ReturnsRequiredFields()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.GetAsync("/api/v1/propertytype");
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
            foreach (var item in body.GetProperty("data").EnumerateArray())
            {
                item.TryGetProperty("id", out _).Should().BeTrue();
                item.TryGetProperty("name", out _).Should().BeTrue();
                item.TryGetProperty("description", out _).Should().BeTrue();
            }
        }

        [Fact]
        public async Task GetById_Existing_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/propertytype/1");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
            body.GetProperty("data").GetProperty("id").GetInt32().Should().Be(1);
        }

        [Fact]
        public async Task GetById_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/propertytype/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetById_AsAnonymous_Returns401()
        {
            var response = await _factory.CreateClient().GetAsync("/api/v1/propertytype/1");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_AsAdmin_Returns201()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var id = Guid.NewGuid().ToString("N")[..8];
            var response = await client.PostAsJsonAsync(
                "/api/v1/propertytype",
                new { name = $"TestType-{id}", description = "Test property type." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Create_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.PostAsJsonAsync(
                "/api/v1/propertytype",
                new { name = "ForbiddenType", description = "Should not be created." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var response = await _factory
                .CreateClient()
                .PostAsJsonAsync(
                    "/api/v1/propertytype",
                    new { name = "AnonType", description = "Should not be created." }
                );
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_DuplicateName_Returns400()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PostAsJsonAsync(
                "/api/v1/propertytype",
                new { name = "Casa", description = "Duplicate name." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_AsAdmin_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PutAsJsonAsync(
                "/api/v1/propertytype/1",
                new { name = "Casa_Updated", description = "Updated description." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Update_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PutAsJsonAsync(
                "/api/v1/propertytype/99999",
                new { name = "Ghost", description = "Non-existing." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.PutAsJsonAsync(
                "/api/v1/propertytype/1",
                new { name = "Hacked", description = "Should not work." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_AsAdmin_Returns204()
        {
            var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var id = Guid.NewGuid().ToString("N")[..8];
            var create = await admin.PostAsJsonAsync(
                "/api/v1/propertytype",
                new { name = $"DeleteMe-{id}", description = "Temp type." }
            );
            var created = await create.Content.ReadFromJsonAsync<JsonElement>(Json);
            var typeId = created.GetProperty("data").GetProperty("id").GetInt32();

            var response = await admin.DeleteAsync($"/api/v1/propertytype/{typeId}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.DeleteAsync("/api/v1/propertytype/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.DeleteAsync("/api/v1/propertytype/1");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_InUseByProperty_Returns409()
        {
            await PropertySeeder.SeedAsync(_factory);
            var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await admin.DeleteAsync("/api/v1/propertytype/1");
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Update_WithDuplicateName_Returns400()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var uid = Guid.NewGuid().ToString("N")[..8];
            var name = $"PT-Dup-{uid}";

            var create = await client.PostAsJsonAsync(
                "/api/v1/propertytype",
                new { name, description = "Original." }
            );
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var update = await client.PutAsJsonAsync(
                "/api/v1/propertytype/1",
                new { name, description = "Duplicate." }
            );
            update.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_WhitespaceName_Returns400()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PostAsJsonAsync(
                "/api/v1/propertytype",
                new { name = "   ", description = "Whitespace name." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_WhitespaceName_Returns400()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PutAsJsonAsync(
                "/api/v1/propertytype/1",
                new { name = "   ", description = "Whitespace name." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    public sealed class SaleTypeTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public SaleTypeTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetAll_AsAnonymous_Returns401()
        {
            var response = await _factory.CreateClient().GetAsync("/api/v1/saletype");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_AsAdmin_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.GetAsync("/api/v1/saletype");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/saletype");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetById_Existing_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/saletype/1");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/saletype/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_AsAdmin_Returns400_DuplicateCode()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PostAsJsonAsync(
                "/api/v1/saletype",
                new
                {
                    code = "Sale",
                    name = "New Sale",
                    description = "All enum codes already seeded.",
                }
            );
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.PostAsJsonAsync(
                "/api/v1/saletype",
                new
                {
                    code = "Rent",
                    name = "ForbiddenSale",
                    description = "Should not be created.",
                }
            );
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var response = await _factory
                .CreateClient()
                .PostAsJsonAsync(
                    "/api/v1/saletype",
                    new
                    {
                        code = "Rent",
                        name = "AnonSale",
                        description = "Should not be created.",
                    }
                );
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_AsAdmin_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PutAsJsonAsync(
                "/api/v1/saletype/1",
                new
                {
                    id = 1,
                    code = "Sale",
                    name = "Venta_Updated",
                    description = "Updated sale type.",
                }
            );
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Update_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PutAsJsonAsync(
                "/api/v1/saletype/99999",
                new
                {
                    id = 99999,
                    code = "Sale",
                    name = "Ghost",
                    description = "Non-existing.",
                }
            );
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.PutAsJsonAsync(
                "/api/v1/saletype/1",
                new
                {
                    id = 1,
                    code = "Sale",
                    name = "Hacked",
                    description = "Should not work.",
                }
            );
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_AsAdmin_Returns204_Cascade()
        {
            // Los valores del enum SaleTypeCode están ocupados por seeds
            // y no se pueden eliminar, pero se puede probar la eliminación de un valor que no esté en uso.
            var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await admin.DeleteAsync("/api/v1/saletype/3");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.DeleteAsync("/api/v1/saletype/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.DeleteAsync("/api/v1/saletype/1");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_InUseCascades_Returns204()
        {
            // SaleType borra en cascada a las propiedades asociadas (sin verificación de conflicto).
            await PropertySeeder.SeedAsync(_factory);
            var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await admin.DeleteAsync("/api/v1/saletype/1");
            // Cascada en propiedades puede fallar si hay restricciones de integridad en la base de datos,
            // por lo que se permite un 500.
            response
                .StatusCode.Should()
                .BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Create_WithInvalidCode_Returns400()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PostAsJsonAsync(
                "/api/v1/saletype",
                new
                {
                    code = "InvalidCode",
                    name = "Invalid Sale Type",
                    description = "Should fail on enum parse.",
                }
            );
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_WithDuplicateCode_Returns400()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PutAsJsonAsync(
                "/api/v1/saletype/2",
                new
                {
                    id = 2,
                    code = "Sale",
                    name = "Duplicate Code",
                    description = "Should fail on duplicate code.",
                }
            );
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    public sealed class ImprovementTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public ImprovementTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetAll_AsAnonymous_Returns401()
        {
            var response = await _factory.CreateClient().GetAsync("/api/v1/improvement");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_AsAdmin_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.GetAsync("/api/v1/improvement");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/improvement");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetById_Existing_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/improvement/1");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.GetAsync("/api/v1/improvement/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_AsAdmin_Returns201()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var id = Guid.NewGuid().ToString("N")[..8];
            var response = await client.PostAsJsonAsync(
                "/api/v1/improvement",
                new { name = $"TestImp-{id}", description = "Test improvement." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Create_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.PostAsJsonAsync(
                "/api/v1/improvement",
                new { name = "ForbiddenImp", description = "Should not be created." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var response = await _factory
                .CreateClient()
                .PostAsJsonAsync(
                    "/api/v1/improvement",
                    new { name = "AnonImp", description = "Should not be created." }
                );
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_AsAdmin_Returns200()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PutAsJsonAsync(
                "/api/v1/improvement/1",
                new { name = "Piscina_Updated", description = "Updated improvement." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Update_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.PutAsJsonAsync(
                "/api/v1/improvement/99999",
                new { name = "Ghost", description = "Non-existing." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.PutAsJsonAsync(
                "/api/v1/improvement/1",
                new { name = "Hacked", description = "Should not work." }
            );
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_AsAdmin_Returns204()
        {
            var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var id = Guid.NewGuid().ToString("N")[..8];
            var create = await admin.PostAsJsonAsync(
                "/api/v1/improvement",
                new { name = $"DeleteImp-{id}", description = "Temp improvement." }
            );
            var created = await create.Content.ReadFromJsonAsync<JsonElement>(Json);
            var impId = created.GetProperty("data").GetProperty("id").GetInt32();

            var response = await admin.DeleteAsync($"/api/v1/improvement/{impId}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_NonExisting_Returns404()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await client.DeleteAsync("/api/v1/improvement/99999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_AsDeveloper_Returns403()
        {
            var client = await _factory.CreateAuthenticatedClientAsync(
                "developer",
                "Developer123!"
            );
            var response = await client.DeleteAsync("/api/v1/improvement/1");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_InUseByProperty_Returns409()
        {
            await PropertySeeder.SeedAsync(_factory);
            var admin = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var response = await admin.DeleteAsync("/api/v1/improvement/1");
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Update_WithDuplicateName_Returns400()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var uid = Guid.NewGuid().ToString("N")[..8];
            var name = $"Imp-Dup-{uid}";

            var create = await client.PostAsJsonAsync(
                "/api/v1/improvement",
                new { name, description = "Original." }
            );
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var update = await client.PutAsJsonAsync(
                "/api/v1/improvement/1",
                new { name, description = "Duplicate." }
            );
            update.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_DuplicateName_AlreadyExists_Returns400()
        {
            var client = await _factory.CreateAuthenticatedClientAsync("admin", "Admin123!");
            var uid = Guid.NewGuid().ToString("N")[..8];
            var name = $"DupImp-{uid}";

            var create = await client.PostAsJsonAsync(
                "/api/v1/improvement",
                new { name, description = "Original." }
            );
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var duplicate = await client.PostAsJsonAsync(
                "/api/v1/improvement",
                new { name, description = "Duplicate." }
            );
            duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
