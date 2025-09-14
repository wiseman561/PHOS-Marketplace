using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In-memory storage for entitlements
var entitlements = new ConcurrentDictionary<string, Entitlement>();

// Health check endpoint
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck")
    .WithTags("Health");

// Service info endpoint
app.MapGet("/api/info", () => Results.Ok(new { name = "entitlements", version = "0.1.0" }))
    .WithName("ServiceInfo")
    .WithTags("Info");

// List all entitlements
app.MapGet("/entitlements", () => Results.Ok(entitlements.Values.ToArray()))
    .WithName("ListEntitlements")
    .WithTags("Entitlements");

// Grant new entitlement
app.MapPost("/entitlements", (GrantRequest request) =>
{
    var id = Guid.NewGuid().ToString();
    var issuedAt = DateTimeOffset.UtcNow;
    var ttlDays = request.TtlDays ?? 30;
    var expiresAt = issuedAt.AddDays(ttlDays);
    
    var entitlement = new Entitlement(
        Id: id,
        UserId: request.UserId,
        AppId: request.AppId,
        Sku: request.Sku ?? "",
        Status: "active",
        IssuedAt: issuedAt,
        ExpiresAt: expiresAt
    );
    
    entitlements[id] = entitlement;
    
    return Results.Created($"/entitlements/{id}", entitlement);
})
.WithName("GrantEntitlement")
.WithTags("Entitlements");

// Get specific entitlement
app.MapGet("/entitlements/{id}", (string id) =>
{
    if (entitlements.TryGetValue(id, out var entitlement))
    {
        return Results.Ok(entitlement);
    }
    
    return Results.NotFound(new ErrorResponse("not_found"));
})
.WithName("GetEntitlement")
.WithTags("Entitlements");

// Check entitlement
app.MapPost("/entitlements/check", (CheckRequest request) =>
{
    var now = DateTimeOffset.UtcNow;
    
    var matchingEntitlement = entitlements.Values.FirstOrDefault(e =>
        e.UserId == request.UserId &&
        e.AppId == request.AppId &&
        (string.IsNullOrEmpty(request.Sku) || e.Sku == request.Sku) &&
        e.Status == "active" &&
        (e.ExpiresAt == null || e.ExpiresAt > now));
    
    if (matchingEntitlement != null)
    {
        return Results.Ok(new CheckResponse(true, "active"));
    }
    
    // Check if there's an expired entitlement
    var expiredEntitlement = entitlements.Values.FirstOrDefault(e =>
        e.UserId == request.UserId &&
        e.AppId == request.AppId &&
        (string.IsNullOrEmpty(request.Sku) || e.Sku == request.Sku) &&
        e.ExpiresAt != null && e.ExpiresAt <= now);
    
    var reason = expiredEntitlement != null ? "expired" : "not_found";
    return Results.Ok(new CheckResponse(false, reason));
})
.WithName("CheckEntitlement")
.WithTags("Entitlements");

app.Run();

// Record types
record Entitlement(string Id, string UserId, string AppId, string Sku, string Status, DateTimeOffset IssuedAt, DateTimeOffset? ExpiresAt);
record GrantRequest(string UserId, string AppId, string? Sku, int? TtlDays);
record CheckRequest(string UserId, string AppId, string? Sku);
record CheckResponse(bool Allowed, string Reason);
record ErrorResponse(string Error);

public partial class Program { }
