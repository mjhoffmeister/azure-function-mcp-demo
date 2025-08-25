using Microsoft.Extensions.Configuration;

namespace AzFuncMcpDemo.Cli;

/// <summary>
/// Strongly-typed configuration for the Spell Chat CLI.
/// </summary>
public sealed class SpellChatConfig
{
    /// <summary>
    /// The Azure OpenAI endpoint (e.g., https://my-aoai.openai.azure.com).
    /// </summary>
    public required string AzureOpenAiEndpoint { get; init; }

    /// <summary>
    /// The Azure OpenAI model deployment name (e.g., gpt-4o).
    /// </summary>
    public required string AzureOpenAiDeployment { get; init; }

    /// <summary>
    /// The MCP SSE server URL
    /// (e.g., http://localhost:7071/runtime/webhooks/mcp/sse).
    /// </summary>
    public required string McpSseUrl { get; init; }

    /// <summary>
    /// Loads and validates configuration from the provided
    /// <see cref="IConfiguration"/>.
    /// Supports flat keys from appsettings.json or environment variables.
    /// </summary>
    /// <param name="config">The configuration root.</param>
    /// <returns>A validated <see cref="SpellChatConfig"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a required value is missing.
    /// </exception>
    public static SpellChatConfig Load(IConfiguration config)
    {
        // We intentionally read the well-known flat keys so callers can keep
        // simple appsettings.json and/or environment variables.
        string endpoint = GetRequiredConfigValue(
            config["AZURE_OPENAI_ENDPOINT"], "AZURE_OPENAI_ENDPOINT")
                .TrimEnd('/');

        string deployment = GetRequiredConfigValue(
            config["AZURE_OPENAI_DEPLOYMENT"], "AZURE_OPENAI_DEPLOYMENT");
            
        string sseUrl = GetRequiredConfigValue(
            config["MCP_SSE_URL"], "MCP_SSE_URL").Trim();

        return new SpellChatConfig
        {
            AzureOpenAiEndpoint = endpoint,
            AzureOpenAiDeployment = deployment,
            McpSseUrl = sseUrl
        };
    }

    private static string GetRequiredConfigValue(string? value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing required configuration: {key}. Provide it in " +
                "appsettings.json or as an environment variable.");
        }

        return value;
    }
}
