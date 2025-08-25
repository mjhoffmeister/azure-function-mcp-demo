using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;
using AzFuncMcpDemo.Cli;

// Spell Chat CLI using Semantic Kernel + Azure OpenAI + MCP tools from a local
// Function app

// Load configuration from appsettings.json and/or environment variables
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

SpellChatConfig appConfig = SpellChatConfig.Load(config);

// Build Kernel with Azure OpenAI chat completion using DefaultAzureCredential
DefaultAzureCredential credential = new();
IKernelBuilder builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: appConfig.AzureOpenAiDeployment,
    endpoint: appConfig.AzureOpenAiEndpoint,
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
        "SpellTools",
        new Uri(appConfig.McpSseUrl),
        cancellationToken: cts.Token);

    Console.WriteLine(
        "Imported MCP tools from SSE server into kernel (plugin: SpellTools)");
}
catch (Exception ex)
{
    Console.WriteLine(
        "[error] Failed to import MCP tools over SSE.\n" +
        $"URL: {appConfig.McpSseUrl}\n" +
        "Tips: Ensure the Functions host is running and the route is the \n" +
        "runtime/webhooks variant. " +
        "Example: http://localhost:7071/runtime/webhooks/mcp/sse\n" +
        $"Details: {ex.Message}");

    return;
}

// Create ChatCompletionAgent with concise instructions and auto tool invocation
ChatCompletionAgent agent = new()
{
    Name = "SpellChat",
    Instructions =
        "You are SpellChat. Be concise. When the user asks to save, " +
        "retrieve, or list spells, you MUST call the appropriate tool " +
        "(saveSpell, getSpell, listSpells) instead of describing the action. " +
        "Prefer tool results over speculation.",
    Kernel = kernel
};

// Maintain a single conversation thread across turns
AgentThread thread = new ChatHistoryAgentThread();

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

    try
    {
        // Configure auto function-calling and invoke the agent with the user's
        // message
        OpenAIPromptExecutionSettings exec = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        KernelArguments kernelArgs = new(exec);
        AgentInvokeOptions options = new() { KernelArguments = kernelArgs };

        ChatMessageContent userMessage = new(AuthorRole.User, input);

        await foreach (var response in agent.InvokeAsync(
            userMessage, thread, options, cts.Token))
        {
            ChatMessageContent? msg = response.Message;
            if (!string.IsNullOrWhiteSpace(msg?.Content))
            {
                Console.WriteLine(msg.Content);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[error] {ex.Message}");
    }
}