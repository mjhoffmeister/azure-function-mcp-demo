# MCP Function Apps Demo Spec

## Summary
Design and implement a minimal, readable Azure Functions-based remote MCP server (Preview) in .NET 8 isolated worker that mocks a spellbook (save/read spells), plus a simple .NET console chat client using Semantic Kernel that connects to and uses the MCP server. Two projects:
- AzFuncMcpDemo.Function: Azure Functions app exposing MCP tools over SSE endpoint `/runtime/webhooks/mcp/sse`.
- AzFuncMcpDemo.Cli: Console chat app that registers the remote MCP server with Semantic Kernel and allows simple chats that call the MCP tools.

The demo must run locally and be ready to deploy to Azure. No real backend; in-memory state for local dev, but structure the function to show where persistence would live.

## Goals and non-goals
- Goals
  - Showcase Azure Functions MCP bindings and SSE webhook endpoint.
  - Provide two MCP tools: saveSpell(name, incantation, effect) and getSpell(name).
  - Keep code small, idiomatic, and easy to read, with clear logging.
  - Show local run: `func start` and client chat that can call MCP tools.
  - Include pointers for Azure deployment and auth with function keys.
- Non-goals
  - No production persistence or auth flows (only function key). No APIM/VNET.
  - No UI beyond console.

## Architecture
- Azure Functions isolated worker (.NET 8). Uses MCP tool trigger attributes to expose tools.
- SSE transport exposed by Azure Functions MCP extension at `/runtime/webhooks/mcp/sse`.
- Client uses Semantic Kernel to register the MCP server as a plugin (SSE over HTTP). Chat loop queries LLM and can call tools.

## Endpoints and tools
- SSE: `/runtime/webhooks/mcp/sse` (local: http://localhost:7071/runtime/webhooks/mcp/sse)
- Tools
  - saveSpell
    - Inputs: name (string), incantation (string), effect (string)
    - Output: string confirmation message
  - getSpell
    - Inputs: name (string)
    - Output: object { name, incantation, effect } or error message if not found

## Data model
- Spell: { name: string, incantation: string, effect: string }
- Storage: in-memory Dictionary<string, Spell> for the life of the host process (mock only). Keep in a singleton service registered into DI.

## Projects and structure
- src/AzFuncMcpDemo.Function/
  - Program.cs: configure Functions isolated worker with ASP.NET Core HTTP integration per docs.
  - host.json, local.settings.json (dev only, not committed)
  - SpellRepository.cs: in-memory repository (thread-safe) implementing the repository pattern (ISpellRepository).
  - Functions/Mcp/SaveSpellFunction.cs: [Function("SaveSpell")] with [McpToolTrigger] and [McpToolProperty] inputs.
  - Functions/Mcp/GetSpellFunction.cs: [Function("GetSpell")] with [McpToolTrigger].
  - Logging and simple validation.
- src/AzFuncMcpDemo.Cli/
  - Program.cs: build Semantic Kernel, register remote MCP server (SSE).
  - Chat loop: user types messages; the kernel can call MCP tools; simple instructions on how to call tools (e.g., "save a spell named fireball…").

## Dependencies
- AzFuncMcpDemo.Function
  - Microsoft.Azure.Functions.Worker
  - Microsoft.Azure.Functions.Worker.Sdk
  - Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore (for ASP.NET Core integration)
  - ModelContextProtocol.Functions (MCP Functions extension preview)
  - Microsoft.Extensions.Logging
- AzFuncMcpDemo.Cli
  - Semantic Kernel (Microsoft.SemanticKernel)
  - Microsoft.SemanticKernel.Agents (for ChatCompletionAgent)
  - Microsoft.Extensions.AI and Microsoft.Extensions.AI.OpenAI (Azure AI Foundry integration)
  - Azure.AI.OpenAI (underlying client) and Azure.Identity (DefaultAzureCredential)
  - ModelContextProtocol (MCP C# SDK) if needed by SK for SSE plugin support

Note: Package names and versions are subject to preview updates; pin latest preview versions when implementing.

## Local development
- Prereqs: .NET 8 SDK, Azure Functions Core Tools (>= 4.0.7030).
- Function app
  - local.settings.json Values:
    - FUNCTIONS_WORKER_RUNTIME=dotnet-isolated
    - AzureWebJobsStorage=UseDevelopmentStorage=true (not required for mock persistence, but standard)
  - Run: `func start` in `src/AzFuncMcpDemo.Function`
  - SSE URL: http://localhost:7071/runtime/webhooks/mcp/sse
- Console app
  - Config points the MCP plugin to the local SSE URL; no headers/keys needed locally.

## Azure deployment (optional)
- Endpoint: https://<funcapp>.azurewebsites.net/runtime/webhooks/mcp/sse
- Clients must include x-functions-key header or `?code=` query with the system key named `mcp_extension`.
- Obtain key: `az functionapp keys list -g <rg> -n <funcapp>`.
- See Azure Developer CLI (azd) for infra if desired; not required for this demo.

## Implementation details
- Function tools
  - Each tool is implemented in its own file/class under `Functions/Mcp/` (namespace can mirror folder path).
    - [Function("SaveSpell")] in `Mcp/SaveSpellFunction.cs` with [McpToolTrigger] and [McpToolProperty] inputs
    - [Function("GetSpell")] in `Mcp/GetSpellFunction.cs` with [McpToolTrigger]
  - Prefer the standard "Function" suffix for Azure Functions consistency; the `Mcp` folder/namespace makes the MCP context explicit. Add a brief XML summary noting it's an MCP tool.
  - Use DI to inject ISpellRepository; store/update Dictionary in a thread-safe way. Repository interface example:
    - ISpellRepository
      - Task SaveAsync(Spell spell)
      - Task<Spell?> GetAsync(string name)
  - Return simple types (string/object) to keep payloads trivial.
- Client
  - Use Semantic Kernel's ChatCompletionAgent for the chat loop and tool calling.
  - Configure Azure AI Foundry chat completion via Microsoft.Extensions.AI with DefaultAzureCredential.
    - Read config from environment variables (example): AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_DEPLOYMENT (or model name), optional AZURE_TENANT_ID for auth context.
  - Register the remote MCP server as a plugin over SSE using SK's MCP plugin support, pointing to the Function SSE URL.
  - Minimal loop: read user input, send to agent, stream/print responses; include basic logging of invoked MCP tools.

## Testing
- Manual: run function locally; in another terminal, run CLI; prompt to save and then fetch a spell.
- Optional: Use MCP Inspector to list tools and invoke.

## Risks and mitigations
- Preview package or API changes: isolate bindings and leave TODO comments with links to official docs.
- Network/auth: local uses no key; Azure requires system key.

## References
- Azure Functions isolated worker HTTP: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#http-trigger
- Triggers/bindings overview: https://learn.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings
- Remote MCP on Azure Functions (sample): https://github.com/Azure-Samples/remote-mcp-functions-dotnet
- .NET MCP with Semantic Kernel: https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-mcp-plugins
- .NET + MCP overview: https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp
