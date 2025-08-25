using System.Collections.Concurrent;
using AzFuncMcpDemo.Function.Models;

namespace AzFuncMcpDemo.Function.Repositories;

/// <summary>
/// In-memory spell repository backed by a concurrent dictionary.
/// Seeds a few default spells on construction.
/// </summary>
public sealed class SpellRepository : ISpellRepository
{
    private readonly ConcurrentDictionary<string, Spell> _spells = new(
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellRepository"/> class.
    /// </summary>
    public SpellRepository()
    {
        SeedDefaults();
    }

    /// <summary>
    /// Saves or updates a spell.
    /// </summary>
    /// <param name="spell">The spell to save.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task SaveAsync(Spell spell, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(spell);
        if (string.IsNullOrWhiteSpace(spell.Name))
        {
            throw new ArgumentException(
                "Spell name is required", nameof(spell));
        }

        if (string.IsNullOrWhiteSpace(spell.Incantation))
        {
            throw new ArgumentException(
                "Spell incantation is required", nameof(spell));
        }

        if (string.IsNullOrWhiteSpace(spell.Effect))
        {
            throw new ArgumentException(
                "Spell effect is required", nameof(spell));
        }

        _spells[spell.Name] = spell;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a spell by name.
    /// </summary>
    /// <param name="name">The spell name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The spell if found; otherwise, null.</returns>
    public Task<Spell?> GetAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult<Spell?>(null);

        _spells.TryGetValue(name, out Spell? spell);

        return Task.FromResult(spell);
    }

    /// <summary>
    /// Gets all spells in the repository.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of spells.</returns>
    public Task<IReadOnlyList<Spell>> GetAllAsync(
        CancellationToken ct = default)
    {
        IReadOnlyList<Spell> list = _spells.Values.ToList();
        return Task.FromResult(list);
    }

    private void SeedDefaults()
    {
        _spells.TryAdd("fireball", new Spell
        {
            Name = "fireball",
            Incantation = "Ignis globus",
            Effect = "Hurls a flaming sphere at a target"
        });

        _spells.TryAdd("lumos", new Spell
        {
            Name = "lumos",
            Incantation = "Lumos",
            Effect = "Emits light to illuminate dark places"
        });

        _spells.TryAdd("protego", new Spell
        {
            Name = "protego",
            Incantation = "Protego",
            Effect = "Conjures a protective barrier"
        });

        _spells.TryAdd("accio", new Spell
        {
            Name = "accio",
            Incantation = "Accio",
            Effect = "Summons an object to the caster"
        });

        _spells.TryAdd("expelliarmus", new Spell
        {
            Name = "expelliarmus",
            Incantation = "Expelliarmus",
            Effect = "Disarms an opponent"
        });
    }
}
