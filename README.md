# Azure Functions MCP Demo

A minimal demo showing how a .NET Azure Functions app exposes Model Context
Protocol (MCP) tools over Server‑Sent Events (SSE), and a .NET CLI that uses
Semantic Kernel with Azure OpenAI to call those tools.

## What’s inside
- Azure Functions (isolated worker) hosting MCP tools:
  - saveSpell, getSpell, listSpells
  - In‑memory repository seeded with a few spells
- .NET CLI using Semantic Kernel + Azure OpenAI
  - Loads config from `appsettings.json` (with env‑var overrides)
  - Imports MCP tools from the Functions SSE endpoint

## Prerequisites
- .NET SDK (matching the solution’s target frameworks)
- Azure Functions Core Tools
- Azurite (for local storage)
- Azure login that can authenticate DefaultAzureCredential (for Azure OpenAI)

## Quick start
1) Start local storage (Azurite)
- On Windows, Azurite can be installed via npm or the VS Code extension. Ensure it’s running.

2) Run the Function app (MCP SSE server)
- From repo root:
  - `cd src/AzFuncMcpDemo/AzFuncMcpDemo.Function`
  - `func start`
- The SSE endpoint will be available at:
  - `http://localhost:7071/runtime/webhooks/mcp/sse`

3) Configure the CLI
- Copy sample settings and edit your values:
  - `src/AzFuncMcpDemo/AzFuncMcpDemo.Cli/appsettings.sample.json` → `appsettings.json`
- Set:
  - `AZURE_OPENAI_ENDPOINT`: your Azure OpenAI endpoint
  - `AZURE_OPENAI_DEPLOYMENT`: your model deployment (e.g., gpt-4o)
  - `MCP_SSE_URL`: `http://localhost:7071/runtime/webhooks/mcp/sse`

4) Run the CLI
- From repo root:
  - `cd src/AzFuncMcpDemo/AzFuncMcpDemo.Cli`
  - `dotnet run`
- Try prompts:
  - "List all spells"
  - "Add a spell named frostbolt with incantation 'Glacies Telum' and effect 'launches a shard of ice'"
  - "What’s the incantation for accio?"

## Notes
- The Function app seeds a few spells on startup.
- The CLI fails fast if required config is missing.
- `appsettings.json` is git‑ignored; the sample file is tracked.