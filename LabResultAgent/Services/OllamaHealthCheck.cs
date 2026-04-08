using LabResultAgent.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LabResultAgent.Services;

/// <summary>
/// Health check that verifies connectivity to the Ollama server.
/// </summary>
public class OllamaHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly AgentOptions _options;

    public OllamaHealthCheck(IHttpClientFactory httpClientFactory, IOptions<AgentOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient("OllamaHealth");
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_options.OllamaUrl.TrimEnd('/')}/api/tags",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy($"Ollama is reachable at {_options.OllamaUrl}");
            }

            return HealthCheckResult.Degraded(
                $"Ollama returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Cannot reach Ollama at {_options.OllamaUrl}", ex);
        }
    }
}
