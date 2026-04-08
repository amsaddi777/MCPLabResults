using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LabResultAgent.Services
{
    internal sealed class OllamaModelOverrideHandler : DelegatingHandler
    {
        private readonly string _modelName;
        private static readonly Regex ModelRegex = new Regex("\"model\"\\s*:\\s*\"(?<m>.*?)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public OllamaModelOverrideHandler(string modelName) => _modelName = modelName;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri != null && request.RequestUri.AbsolutePath.Contains("/api/chat") && request.Content != null)
            {
                var contentType = request.Content.Headers.ContentType?.MediaType;
                if (string.Equals(contentType, "application/json", System.StringComparison.OrdinalIgnoreCase) || contentType == null)
                {
                    var body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(_modelName))
                    {
                        var replaced = ModelRegex.Replace(body, $"\"model\": \"{_modelName}\"");
                        if (replaced != body)
                        {
                            request.Content = new StringContent(replaced, Encoding.UTF8, "application/json");
                        }
                    }
                }
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
