using Serilog;
using LabResultMcpServer;
using LabResultMcpServer.Models;
using LabResultMcpServer.Services;
using LabResultMcpServer.Middleware;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// If you need to use a custom tnsnames.ora location, set TNS_ADMIN here
var tnsAdmin = builder.Configuration["Oracle:TnsAdmin"];
if (!string.IsNullOrEmpty(tnsAdmin))
{
    Environment.SetEnvironmentVariable("TNS_ADMIN", tnsAdmin);
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure CORS to allow the LabResultApp on localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddScoped<LabResultService>();
builder.Services.AddScoped<LabResultTool>();

var app = builder.Build();

// Add API key authentication middleware
// Enable CORS for browser requests from the app before other middleware
app.UseCors("AllowLocalhost3000");

// Add API key authentication middleware
app.UseApiKeyAuthentication();

// Add request/response logging middleware (logs headers and bodies)
app.UseRequestResponseLogging();

// Map the MCP endpoint at /mcp so clients that target /mcp succeed
app.MapMcp("/mcp");

// Expose a simple health-check endpoint used by the frontend and docker-compose
app.MapGet("/health", () => Results.Ok());

app.Run("http://localhost:3001");
