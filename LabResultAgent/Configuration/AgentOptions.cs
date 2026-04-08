namespace LabResultAgent.Configuration;

/// <summary>
/// Configuration options for the AI agent, including Ollama and MCP server settings.
/// Bound from the "Agent" section of appsettings.json.
/// </summary>
public class AgentOptions
{
    public const string SectionName = "Agent";

    /// <summary>URL of the remote Ollama server (e.g., http://ollama:11434).</summary>
    public string OllamaUrl { get; set; } = "http://3abbid.com:47382/";

    /// <summary>Ollama model name to use for chat completions (e.g., llama3.1).</summary>
    public string ModelName { get; set; } = "hf.co/mradermacher/HealthCare-Reasoning-Assistant-Llama-3.1-8B-HF-GGUF:Q8_0";

    /// <summary>URL of the LabResultMcpServer (e.g., http://labresult-mcp:3001).</summary>
    public string McpServerUrl { get; set; } = "http://localhost:3001";

    /// <summary>API key used to authenticate with the MCP server.</summary>
    public string McpApiKey { get; set; } = string.Empty;

    /// <summary>Maximum tokens to return from the LLM per response.</summary>
    public int MaxTokens { get; set; } = 4096;
}
