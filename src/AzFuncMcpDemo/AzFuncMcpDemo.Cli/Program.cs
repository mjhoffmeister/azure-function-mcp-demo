using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;

// Spell Chat CLI using Semantic Kernel + Azure OpenAI + MCP tools from a local
// Function app

// Load configuration from appsettings.json and/or environment variables
const string KeyAoaiEndpoint = "AZURE_OPENAI_ENDPOINT";
const string KeyAoaiDeployment = "AZURE_OPENAI_DEPLOYMENT";
const string KeyMcpSseUrl = "MCP_SSE_URL";
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

string azureOpenAiEndpoint = GetRequiredConfigValue(
    config[KeyAoaiEndpoint], KeyAoaiEndpoint).TrimEnd('/');

string azureOpenAiDeployment = GetRequiredConfigValue(
    config[KeyAoaiDeployment], KeyAoaiDeployment);

string mcpSseUrl = GetRequiredConfigValue(
    config[KeyMcpSseUrl], KeyMcpSseUrl).Trim();

// Build Kernel with Azure OpenAI chat completion using DefaultAzureCredential
DefaultAzureCredential credential = new();
IKernelBuilder builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: azureOpenAiDeployment,
    endpoint: azureOpenAiEndpoint,
    credentials: credential);

Kernel kernel = builder.Build();

// Support Ctrl+C graceful cancellation
using CancellationTokenSource cts = new();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Import MCP tools from the Functions app over SSE using SK extension
try
{
    await kernel.Plugins.AddMcpFunctionsFromSseServerAsync(
        "SpellTools", new Uri(mcpSseUrl), cancellationToken: cts.Token);

    Console.WriteLine(
        "Imported MCP tools from SSE server into kernel (plugin: SpellTools)");
}
catch (Exception ex)
{
    Console.WriteLine(
        "[error] Failed to import MCP tools over SSE.\n" +
        $"URL: {mcpSseUrl}\n" +
        "Tips: Ensure the Functions host is running and the route is the \n" +
        "runtime/webhooks variant. " +
        "Example: http://localhost:7071/runtime/webhooks/mcp/sse\n" +
        $"Details: {ex.Message}");

    return;
}

// Add chat completion
IChatCompletionService chat = kernel
    .GetRequiredService<IChatCompletionService>();

ChatHistory history = new();

// Provide a concise system prompt guiding the model to use tools
history.AddSystemMessage(
    "You are SpellChat. Be concise. When the user asks to save, retrieve, or " +
    "list spells, use the available tools (saveSpell, getSpell, listSpells). " +
    "Prefer tool results over speculation.");

OpenAIPromptExecutionSettings executionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};

Console.WriteLine("Type your message. Type 'help' for tips, 'exit' to quit.\n");

while (!cts.IsCancellationRequested)
{
    Console.Write("> ");

    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input)) continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

    if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Examples: \n - Save a spell named fireball. " +
        "Incantation: Ignis globus. Effect: Hurls a flaming sphere.\n" +
        " - What spells are available?\n" +
        " - What's the incantation for the accio spell?\n");

        continue;
    }

    history.AddUserMessage(input);

    try
    {
        IReadOnlyList<ChatMessageContent> result = await chat
            .GetChatMessageContentsAsync(
                history, executionSettings, kernel, cts.Token);

        // Append assistant messages to history and print response
        foreach (ChatMessageContent message in result)
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                Console.WriteLine(message.Content);
                history.AddAssistantMessage(message.Content);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[error] {ex.Message}");
    }
}

/// <summary>
/// Gets a required configuration value, throwing an exception if not found.
/// </summary>
static string GetRequiredConfigValue(string? value, string key)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException(
            $"Missing required configuration: {key}. Provide it in " +
            "appsettings.json or as an environment variable.");
    }

    return value;
}