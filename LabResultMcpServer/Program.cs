using Serilog;
using LabResultMcpServer;
using LabResultMcpServer.Models;
using LabResultMcpServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddScoped<LabResultService>();
builder.Services.AddScoped<LabResultTool>();

var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:3001");
