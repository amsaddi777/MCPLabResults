using LabResultAgent.Configuration;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LabResultAgent.Services;

/// <summary>
/// Manages the MCP client connection to the LabResultMcpServer.
/// Provides methods to call MCP tools as simple async methods.
/// </summary>
public class McpClientService : IAsyncDisposable
{
    private readonly AgentOptions _options;
    private readonly ILogger<McpClientService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private McpClient? _client;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public McpClientService(
        IOptions<AgentOptions> options,
        ILogger<McpClientService> logger,
        ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Ensures the MCP client is connected and initialized.
    /// Uses Streamable HTTP transport.
    /// </summary>
    private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized && _client != null) return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized && _client != null) return;

            _logger.LogInformation("Connecting to MCP server at {Url}", _options.McpServerUrl);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.McpApiKey);

            var transportOptions = new HttpClientTransportOptions
            {
                Endpoint = new Uri($"{_options.McpServerUrl.TrimEnd('/')}/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp,
                AdditionalHeaders = new Dictionary<string, string>
                {
                    ["X-API-Key"] = _options.McpApiKey
                }
            };

            var transport = new HttpClientTransport(
                transportOptions,
                httpClient,
                _loggerFactory,
                ownsHttpClient: true);

            _client = await McpClient.CreateAsync(
                transport,
                clientOptions: null,
                _loggerFactory,
                cancellationToken);

            _initialized = true;
            _logger.LogInformation("MCP client connected successfully (Streamable HTTP)");
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Calls the fetch_patient_lab_results MCP tool and returns the raw JSON result.
    /// </summary>
    public async Task<string> FetchPatientLabResultsAsync(
        string patientId,
        string? nda = null,
        string? startDate = null,
        string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        _logger.LogInformation(
            "Calling MCP tool fetch_patient_lab_results for patient {PatientId}",
            patientId);

        var arguments = new Dictionary<string, object?>
        {
            ["patientId"] = patientId
        };

        if (!string.IsNullOrWhiteSpace(nda))
            arguments["nda"] = nda;

        if (!string.IsNullOrWhiteSpace(startDate) || !string.IsNullOrWhiteSpace(endDate))
        {
            var dateRange = new Dictionary<string, object?>();
            if (!string.IsNullOrWhiteSpace(startDate))
                dateRange["start"] = startDate;
            if (!string.IsNullOrWhiteSpace(endDate))
                dateRange["end"] = endDate;
            arguments["dateRange"] = dateRange;
        }

        try
        {
            var result = await _client!.CallToolAsync(
                "fetch_patient_lab_results",
                arguments,
                progress: null,
                options: null,
                cancellationToken: cancellationToken);

            var textContent = result.Content
                .OfType<TextContentBlock>()
                .Select(c => c.Text)
                .FirstOrDefault() ?? "{}";

            _logger.LogInformation(
                "MCP tool returned {Length} chars for patient {PatientId}",
                textContent.Length, patientId);

            return textContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP tool for patient {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Checks if the MCP server is reachable.
    /// </summary>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureInitializedAsync(cancellationToken);
            var tools = await _client!.ListToolsAsync(options: null, cancellationToken);
            return tools.Any(t => t.Name == "fetch_patient_lab_results");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MCP health check failed");
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is IAsyncDisposable disposable)
            await disposable.DisposeAsync();
        _initLock.Dispose();
    }
}
