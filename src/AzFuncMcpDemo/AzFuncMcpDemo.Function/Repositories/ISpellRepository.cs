using AzFuncMcpDemo.Function.Models;

namespace AzFuncMcpDemo.Function.Repositories;

public interface ISpellRepository
{
    Task SaveAsync(Spell spell, CancellationToken ct = default);
    Task<Spell?> GetAsync(string name, CancellationToken ct = default);
    /// <summary>
    /// Gets all spells in the repository.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of spells.</returns>
    Task<IReadOnlyList<Spell>> GetAllAsync(CancellationToken ct = default);
}
