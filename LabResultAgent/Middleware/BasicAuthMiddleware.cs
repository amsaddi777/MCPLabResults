using System.Net.Http.Headers;
using System.Text;
using LabResultAgent.Configuration;
using Microsoft.Extensions.Options;

namespace LabResultAgent.Middleware;

/// <summary>
/// Basic authentication middleware for the AG-UI endpoint.
/// Validates username/password from the Authorization header.
/// </summary>
public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BasicAuthMiddleware> _logger;

    public BasicAuthMiddleware(RequestDelegate next, ILogger<BasicAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authOptions = context.RequestServices
            .GetRequiredService<IOptions<AuthOptions>>().Value;

        // Skip auth if disabled
        if (!authOptions.Enabled)
        {
            await _next(context);
            return;
        }

        // Skip auth for health and CORS preflight
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        // Extract Authorization header
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            _logger.LogWarning("Request missing Authorization header from {IP}",
                context.Connection.RemoteIpAddress);
            context.Response.StatusCode = 401;
            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"LabResultAgent\"";
            await context.Response.WriteAsJsonAsync(new { error = "Authorization required" });
            return;
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(
                context.Request.Headers.Authorization!);

            if (authHeader.Scheme != "Basic" || string.IsNullOrEmpty(authHeader.Parameter))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid authentication scheme" });
                return;
            }

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

            if (credentials.Length != 2)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid credentials format" });
                return;
            }

            var username = credentials[0];
            var password = credentials[1];

            if (username == authOptions.Username && password == authOptions.Password)
            {
                _logger.LogDebug("Basic auth successful for user {Username}", username);
                await _next(context);
                return;
            }

            _logger.LogWarning("Invalid credentials for user {Username}", username);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid credentials" });
        }
        catch (FormatException)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid authorization header" });
        }
    }
}

public static class BasicAuthExtensions
{
    public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<BasicAuthMiddleware>();
    }
}
