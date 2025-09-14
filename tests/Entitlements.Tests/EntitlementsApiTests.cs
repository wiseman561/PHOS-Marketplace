using Microsoft.AspNetCore.Mvc.Testing;

namespace Entitlements.Tests;

public class EntitlementsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EntitlementsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthz_ReturnsOkWithStatus()
    {
        // Act
        var response = await _client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("status");
        content.Should().Contain("ok");
    }

    [Fact]
    public async Task GetApiInfo_ReturnsServiceInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("name");
        content.Should().Contain("entitlements");
        content.Should().Contain("version");
        content.Should().Contain("0.1.0");
    }

    [Fact]
    public async Task PostEntitlements_CreatesEntitlementAndReturns201()
    {
        // Arrange
        var request = new
        {
            userId = "u1",
            appId = "app.demo",
            sku = "pro",
            ttlDays = 7
        };

        // Act
        var response = await _client.PostAsJsonAsync("/entitlements", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var location = response.Headers.Location?.ToString();
        location.Should().StartWith("/entitlements/");
        
        var entitlement = await response.Content.ReadFromJsonAsync<Entitlement>();
        entitlement.Should().NotBeNull();
        entitlement!.Id.Should().NotBeEmpty();
        entitlement.Id.Should().MatchRegex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");
        entitlement.UserId.Should().Be("u1");
        entitlement.AppId.Should().Be("app.demo");
        entitlement.Sku.Should().Be("pro");
        entitlement.Status.Should().Be("active");
        entitlement.IssuedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        entitlement.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetEntitlements_ReturnsArrayWithAtLeastOneItem()
    {
        // Arrange - Create an entitlement first
        var request = new
        {
            userId = "u1",
            appId = "app.demo",
            sku = "pro",
            ttlDays = 7
        };
        await _client.PostAsJsonAsync("/entitlements", request);

        // Act
        var response = await _client.GetAsync("/entitlements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entitlements = await response.Content.ReadFromJsonAsync<Entitlement[]>();
        entitlements.Should().NotBeNull();
        entitlements!.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetEntitlementById_ReturnsMatchingEntitlement()
    {
        // Arrange - Create an entitlement first
        var request = new
        {
            userId = "u1",
            appId = "app.demo",
            sku = "pro",
            ttlDays = 7
        };
        var createResponse = await _client.PostAsJsonAsync("/entitlements", request);
        var createdEntitlement = await createResponse.Content.ReadFromJsonAsync<Entitlement>();

        // Act
        var response = await _client.GetAsync($"/entitlements/{createdEntitlement!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entitlement = await response.Content.ReadFromJsonAsync<Entitlement>();
        entitlement.Should().NotBeNull();
        entitlement!.Id.Should().Be(createdEntitlement.Id);
        entitlement.UserId.Should().Be("u1");
        entitlement.AppId.Should().Be("app.demo");
        entitlement.Sku.Should().Be("pro");
    }

    [Fact]
    public async Task PostEntitlementsCheck_WithMatchingFields_ReturnsAllowedTrue()
    {
        // Arrange - Create an entitlement first
        var createRequest = new
        {
            userId = "u1",
            appId = "app.demo",
            sku = "pro",
            ttlDays = 7
        };
        await _client.PostAsJsonAsync("/entitlements", createRequest);

        var checkRequest = new
        {
            userId = "u1",
            appId = "app.demo",
            sku = "pro"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/entitlements/check", checkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var checkResponse = await response.Content.ReadFromJsonAsync<CheckResponse>();
        checkResponse.Should().NotBeNull();
        checkResponse!.Allowed.Should().BeTrue();
        checkResponse.Reason.Should().Be("active");
    }

    [Fact]
    public async Task PostEntitlementsCheck_WithNonExistentSku_ReturnsAllowedFalse()
    {
        // Arrange - Create an entitlement first
        var createRequest = new
        {
            userId = "u1",
            appId = "app.demo",
            sku = "pro",
            ttlDays = 7
        };
        await _client.PostAsJsonAsync("/entitlements", createRequest);

        var checkRequest = new
        {
            userId = "u1",
            appId = "app.demo",
            sku = "gold"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/entitlements/check", checkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var checkResponse = await response.Content.ReadFromJsonAsync<CheckResponse>();
        checkResponse.Should().NotBeNull();
        checkResponse!.Allowed.Should().BeFalse();
        checkResponse.Reason.Should().Be("not_found");
    }
}

// Record types to match the API
public record Entitlement(string Id, string UserId, string AppId, string Sku, string Status, DateTimeOffset IssuedAt, DateTimeOffset? ExpiresAt);
public record CheckResponse(bool Allowed, string Reason);
