namespace LabResultAgent.Configuration;

/// <summary>
/// Configuration for basic authentication on the AG-UI endpoint.
/// Bound from the "Auth" section of appsettings.json.
/// </summary>
public class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Username for basic authentication.</summary>
    public string Username { get; set; } = "admin";

    /// <summary>Password for basic authentication (plain text — use hashing in production).</summary>
    public string Password { get; set; } = "changeme";

    /// <summary>Whether basic authentication is enabled. Disable for development.</summary>
    public bool Enabled { get; set; } = true;
}
