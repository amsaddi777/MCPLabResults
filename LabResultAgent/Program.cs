using LabResultAgent;
using LabResultAgent.Configuration;
using LabResultAgent.Middleware;
using LabResultAgent.Services;
using LabResultAgent.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// ── Configuration ──
builder.Services.Configure<AgentOptions>(
    builder.Configuration.GetSection(AgentOptions.SectionName));
builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection(AuthOptions.SectionName));

var agentConfig = builder.Configuration.GetSection(AgentOptions.SectionName).Get<AgentOptions>()
    ?? new AgentOptions();

// ── Ollama IChatClient with HTTP logging handler ──
// OllamaSharp natively implements IChatClient from Microsoft.Extensions.AI
builder.Services.AddChatClient(sp =>
{
    // Create a delegating handler to log outgoing requests/responses
    var loggingHandler = new LabResultAgent.Services.OllamaHttpLoggingHandler(
        sp.GetRequiredService<ILogger<LabResultAgent.Services.OllamaHttpLoggingHandler>>());

    // Model override handler ensures the configured model string is always used
    var overrideHandler = new LabResultAgent.Services.OllamaModelOverrideHandler(agentConfig.ModelName)
    {
        InnerHandler = loggingHandler
    };

    loggingHandler.InnerHandler = new HttpClientHandler();

    var httpClient = new HttpClient(overrideHandler)
    {
        BaseAddress = new Uri(agentConfig.OllamaUrl)
    };
    // Increase timeout for long-running streaming responses from the Ollama model
    // Default HttpClient.Timeout is 100s; bump to 300s to avoid premature cancellations
    httpClient.Timeout = TimeSpan.FromSeconds(300);

    // Construct the Ollama client using the HttpClient so we can observe traffic
    var ollamaClient = new OllamaApiClient(httpClient, agentConfig.ModelName);

    // Wrap with function invocation middleware for tool calling
    return new ChatClientBuilder(ollamaClient)
        .UseFunctionInvocation()
        .Build();
});

// The HTTP logging delegating handler is implemented in Services/OllamaHttpLoggingHandler.cs

// ── MCP Client Service ──
builder.Services.AddSingleton<McpClientService>();

// ── Health Checks ──
builder.Services.AddHttpClient("OllamaHealth");
builder.Services.AddHealthChecks()
    .AddCheck<OllamaHealthCheck>("ollama");

// ── CORS (for CopilotKit frontend) ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ── Middleware Pipeline ──
app.UseCors();
app.UseBasicAuthentication();

// SSE filter: remove tool-role events from AG-UI streams so frontend
// doesn't display raw tool output while preserving tool messages
// in the agent context for the LLM.
app.UseMiddleware<LabResultAgent.Middleware.FilterSseMiddleware>();

// ── Health endpoint ──
app.MapHealthChecks("/health");

// ── AG-UI Endpoint ──
// This exposes the agent via the AG-UI protocol (HTTP POST + SSE streaming)
// CopilotKit connects to this endpoint to communicate with the agent.
{
    var chatClient = app.Services.GetRequiredService<IChatClient>();
    var mcpClient = app.Services.GetRequiredService<McpClientService>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

    // Create the MCP tool as an AIFunction
    var labResultsTool = FetchLabResultsTool.Create(mcpClient, logger);

    var agent = new ChatClientAgent(
        chatClient,
        SystemPrompts.LabResultAgent,
        "LabResultAgent",
        "Agent that retrieves and summarizes patient lab results.",
        new List<AITool> { labResultsTool },
        loggerFactory,
        app.Services);

    app.MapAGUI("/", agent);
}

Log.Information("LabResultAgent starting on http://localhost:8000");
Log.Information("Ollama endpoint: {OllamaUrl}, Model: {Model}", agentConfig.OllamaUrl, agentConfig.ModelName);
Log.Information("MCP Server: {McpUrl}", agentConfig.McpServerUrl);

app.Run("http://0.0.0.0:8000");
