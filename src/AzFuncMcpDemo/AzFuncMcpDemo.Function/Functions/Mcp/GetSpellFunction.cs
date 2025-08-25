using AzFuncMcpDemo.Function.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace AzFuncMcpDemo.Functions.Mcp;

/// <summary>
/// MCP tool exposed as an Azure Function that retrieves a spell from the in-memory repository.
/// </summary>
public sealed class GetSpellFunction(ILogger<GetSpellFunction> logger, ISpellRepository repository)
{
    private const string ToolName = "get_spell";
    private const string ToolDescription = "Retrieve a spell by name.";
    private const string NameProp = "name";

    [Function("GetSpell")]
    public async Task<object> Run(
        [McpToolTrigger(ToolName, ToolDescription)] ToolInvocationContext toolInvocation,
        [McpToolProperty(NameProp, "string", "Spell name")] string name,
        CancellationToken ct)
    {
        name = (name ?? string.Empty).Trim();
        logger.LogInformation("Getting spell {Name}", name);
        var spell = await repository.GetAsync(name, ct);
        if (spell is null)
        {
            return new { message = $"Spell '{name}' not found." };
        }

        return new { spell.Name, spell.Incantation, spell.Effect };
    }
}
