using AzFuncMcpDemo.Function.Models;

namespace AzFuncMcpDemo.Functions.Mcp;

/// <summary>
/// Response contract for the listSpells MCP tool.
/// </summary>
public sealed class ListSpellsResponse
{
    /// <summary>
    /// Gets the collection of spells.
    /// </summary>
    public required IReadOnlyList<Spell> Spells { get; init; }

    /// <summary>
    /// Gets an optional informational message (e.g., when the list is empty).
    /// </summary>
    public string? Message { get; init; }
}
