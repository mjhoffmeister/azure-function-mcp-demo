using AzFuncMcpDemo.Function.Models;
using AzFuncMcpDemo.Function.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace AzFuncMcpDemo.Functions.Mcp;

/// <summary>
/// MCP tool exposed as an Azure Function that saves a spell in an in-memory repository.
/// </summary>
public sealed class SaveSpellFunction(ILogger<SaveSpellFunction> logger, ISpellRepository repository)
{
    private const string ToolName = "saveSpell";
    private const string ToolDescription = "Save a spell with a name, incantation, and effect.";

    private const string NameProp = "name";
    private const string IncantationProp = "incantation";
    private const string EffectProp = "effect";

    [Function("SaveSpell")]
    public async Task<string> Run(
        [McpToolTrigger(ToolName, ToolDescription)] ToolInvocationContext toolInvocation,
        [McpToolProperty(NameProp, "string", "Spell name")] string name,
        [McpToolProperty(IncantationProp, "string", "Spell incantation")] string incantation,
        [McpToolProperty(EffectProp, "string", "Spell effect")] string effect,
        CancellationToken ct)
    {
        // Normalize inputs and validate
        name = (name ?? string.Empty).Trim();
        incantation = (incantation ?? string.Empty).Trim();
        effect = (effect ?? string.Empty).Trim();

        if (name.Length == 0 || incantation.Length == 0 || effect.Length == 0)
        {
            return "Please provide name, incantation, and effect.";
        }

        logger.LogInformation("Saving spell {Name}", name);
        var spell = new Spell { Name = name, Incantation = incantation, Effect = effect };
        await repository.SaveAsync(spell, ct);
        return $"Saved spell '{name}'.";
    }
}
