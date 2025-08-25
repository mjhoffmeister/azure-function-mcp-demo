using AzFuncMcpDemo.Function.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace AzFuncMcpDemo.Functions.Mcp;

/// <summary>
/// MCP tool that lists all available spells.
/// </summary>
public sealed class ListSpellsFunction(ILogger<ListSpellsFunction> logger, ISpellRepository repository)
{
    private const string ToolName = "listSpells";
    private const string ToolDescription = "List all available spells.";

    [Function("ListSpells")]
    public async Task<object> Run(
        [McpToolTrigger(ToolName, ToolDescription)] ToolInvocationContext toolInvocation,
        CancellationToken ct)
    {
        logger.LogInformation("Listing all spells");

        var spells = await repository.GetAllAsync(ct);

        if (spells.Count == 0)
            return new { message = "No spells are currently available." };

        // Return a simple array of { name, incantation, effect }
        var result = spells.Select(s => new { s.Name, s.Incantation, s.Effect })
            .ToArray();

        return new { spells = result };
    }
}
