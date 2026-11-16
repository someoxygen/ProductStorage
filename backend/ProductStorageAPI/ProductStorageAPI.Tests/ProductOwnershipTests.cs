using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProductStorageAPI.Abstractions;
using ProductStorageAPI.Data;
using ProductStorageAPI.DTOs.Auth;
using ProductStorageAPI.DTOs.Product;
using ProductStorageAPI.Models;

namespace ProductStorageAPI.Tests;

public class ProductOwnershipTests
{
    [Fact]
    public async Task UsersCanOnlyReadAndModifyTheirOwnProducts()
    {
        using var factory = new ApiFactory();
        using var alice = await Register(factory, "alice");
        using var bob = await Register(factory, "bob");

        // A client-supplied owner is ignored; the JWT determines ownership.
        var created = await alice.PostAsJsonAsync("/api/Products", new
        {
            name = "Shared name", price = 12, amount = 3, userId = 2
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var product = (await created.Content.ReadFromJsonAsync<ProductDto>())!;
        var bobCreated = await bob.PostAsJsonAsync("/api/Products", new { name = "Shared name" });
        Assert.Equal(HttpStatusCode.Created, bobCreated.StatusCode);
        var bobProduct = (await bobCreated.Content.ReadFromJsonAsync<ProductDto>())!;

        Assert.Equal(product.Id, Assert.Single((await alice.GetFromJsonAsync<List<ProductDto>>("/api/Products"))!).Id);
        Assert.Equal(bobProduct.Id, Assert.Single((await bob.GetFromJsonAsync<List<ProductDto>>("/api/Products?search=Shared%20name"))!).Id);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/api/Products/{product.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PutAsJsonAsync($"/api/Products/{product.Id}", new { name = "Stolen" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync($"/api/Products/{product.Id}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await alice.PutAsJsonAsync($"/api/Products/{product.Id}", new { name = "Updated", price = 25 })).StatusCode);
        Assert.Equal("Updated", (await alice.GetFromJsonAsync<ProductDto>($"/api/Products/{product.Id}"))!.Name);
        Assert.Equal(HttpStatusCode.OK, (await alice.DeleteAsync($"/api/Products/{product.Id}")).StatusCode);
        Assert.Empty((await alice.GetFromJsonAsync<List<ProductDto>>("/api/Products"))!);
        Assert.Single((await bob.GetFromJsonAsync<List<ProductDto>>("/api/Products"))!);
    }

    [Fact]
    public async Task LegacyProductsRemainStoredButInaccessible()
    {
        using var factory = new ApiFactory();
        using var client = await Register(factory, "owner");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var legacy = new Product { Name = "Legacy" };
        db.Products.Add(legacy);
        await db.SaveChangesAsync();

        Assert.Empty((await client.GetFromJsonAsync<List<ProductDto>>("/api/Products"))!);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/Products/{legacy.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/Products/{legacy.Id}", new { name = "Claimed" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/Products/{legacy.Id}")).StatusCode);
        Assert.True(await db.Products.AnyAsync(p => p.Id == legacy.Id && p.UserId == null));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingOrInvalidUserIdentityCannotAccessAnyProductEndpoint(bool invalidIdentity)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        if (invalidIdentity)
        {
            using var scope = factory.Services.CreateScope();
            var token = scope.ServiceProvider.GetRequiredService<ITokenService>()
                .GenerateToken(new User { Id = 0, Email = "invalid@example.com" });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Products")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Products/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/Products", new { name = "Denied" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsJsonAsync("/api/Products/1", new { name = "Denied" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync("/api/Products/1")).StatusCode);
    }

    [Fact]
    public async Task DatabaseEnforcesUniqueNamesPerOwner()
    {
        using var factory = new ApiFactory();
        using var client = await Register(factory, "owner");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await db.Users.Select(u => u.Id).SingleAsync();
        db.Products.Add(new Product { UserId = userId, Name = "Duplicate" });
        await db.SaveChangesAsync();
        db.Products.Add(new Product { UserId = userId, Name = "Duplicate" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static async Task<HttpClient> Register(ApiFactory factory, string name)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/register", new
        {
            fullName = name, email = $"{name}@example.com", password = "TestPassword123!"
        });
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            connection.Open();
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "OwnershipTestsOnly-NotAProductionSecret-123456789",
                ["Jwt:Issuer"] = "OwnershipTests",
                ["Jwt:Audience"] = "OwnershipTests",
                ["Jwt:ExpiresInMinutes"] = "30"
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) connection.Dispose();
        }
    }
}
