using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LabResultMcpServer.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Log request line and headers
            _logger.LogInformation("Incoming request {Method} {Path}", context.Request.Method, context.Request.Path);

            foreach (var header in context.Request.Headers)
            {
                _logger.LogDebug("Request Header: {Name} = {Value}", header.Key, header.Value.ToString());
            }

            // Read and log request body (if any)
            if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    _logger.LogDebug("Request Body: {Body}", body);
                }
            }

            // Capture the response body
            var originalBodyStream = context.Response.Body;
            await using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Read response
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var respText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation("Response {StatusCode} for {Path}", context.Response.StatusCode, context.Request.Path);
            if (!string.IsNullOrWhiteSpace(respText))
            {
                _logger.LogDebug("Response Body: {Body}", respText);
            }

            // Copy the contents of the new memory stream (which contains the response) to the original stream.
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RequestResponseLoggingMiddleware");
            throw;
        }
    }
}

public static class RequestResponseLoggingExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
