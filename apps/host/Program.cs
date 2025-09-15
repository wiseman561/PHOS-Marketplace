using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/info", () => Results.Ok(new { name = "apps-host", version = "0.1.0" }));

var root = builder.Environment.ContentRootPath;
// Map SDK files under /sdk
var sdkPath = Path.Combine(root, "..", "..", "sdk", "js", "phos-sdk");
app.UseFileServer(new FileServerOptions {
    RequestPath = "/sdk",
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(sdkPath)),
    EnableDefaultFiles = false
});
// Map Hello-World app under /apps/hello-world
var helloPath = Path.Combine(root, "..", "hello-world");
app.UseFileServer(new FileServerOptions {
    RequestPath = "/apps/hello-world",
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(helloPath)),
    EnableDefaultFiles = true // serves index.html by default
});

app.Run();

public partial class Program { }
