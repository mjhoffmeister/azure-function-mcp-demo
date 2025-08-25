using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzFuncMcpDemo.Function.Repositories;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Print the SSE route once for demo clarity
Console.WriteLine(
    "MCP SSE available at http://localhost:7071/runtime/webhooks/mcp/sse");

// DI registrations
builder.Services.AddSingleton<ISpellRepository, SpellRepository>();

builder.Build().Run();
