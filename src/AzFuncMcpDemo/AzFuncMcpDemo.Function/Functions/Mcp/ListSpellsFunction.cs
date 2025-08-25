using AzFuncMcpDemo.Function.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace AzFuncMcpDemo.Functions.Mcp;

/// <summary>
/// MCP tool that lists all available spells.
/// </summary>
public sealed class ListSpellsFunction(
    ILogger<ListSpellsFunction> logger, ISpellRepository repository)
{
    private const string ToolName = "list_spells";
    private const string ToolDescription = "List all available spells.";

    [Function("ListSpells")]
    public async Task<ListSpellsResponse> Run(
        [McpToolTrigger(ToolName, ToolDescription)] ToolInvocationContext toolInvocation,
        CancellationToken ct)
    {
        logger.LogInformation("Listing all spells");

        var spells = await repository.GetAllAsync(ct);

        if (spells.Count == 0)
        {
            return new ListSpellsResponse
            {
                Spells = Array.Empty<AzFuncMcpDemo.Function.Models.Spell>(),
                Message = "No spells are currently available."
            };
        }

        // Return the domain spells directly for simplicity in the demo
        return new ListSpellsResponse { Spells = spells };
    }
}
