using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LabResultAgent.Services
{
    internal sealed class OllamaHttpLoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        public OllamaHttpLoggingHandler(ILogger<OllamaHttpLoggingHandler> logger) => _logger = logger;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ollama HTTP Request: {Method} {Uri}", request.Method, request.RequestUri);
                if (request.Content != null)
                {
                    var reqBody = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogDebug("Ollama Request Body: {Body}", reqBody);
                }

                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Ollama HTTP Response: {StatusCode} for {Uri}", response.StatusCode, request.RequestUri);
                if (response.Content != null)
                {
                    var respBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogDebug("Ollama Response Body: {Body}", respBody);
                }

                return response;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error sending request to Ollama");
                throw;
            }
        }
    }
}
