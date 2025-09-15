using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Load reverse proxy routes/clusters from a separate json file
builder.Configuration.AddJsonFile("appsettings.ReverseProxy.json", optional: false, reloadOnChange: true);

// JWT auth (stub – not enforced yet). We register it now so downstream can be tightened later.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // DEV stub – accepts any well-formed token signed with unknown key; token is optional.
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false
        };
    });

// Add reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Health & service info
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/info", () => Results.Ok(new { name = "gateway", version = "0.1.0" }));

// NOTE: For now we allow anonymous requests through the gateway.
// Later we'll add policies & per-route authorization.
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// YARP reverse proxy middleware
app.MapReverseProxy();

app.Run();

// For WebApplicationFactory
public partial class Program { }
