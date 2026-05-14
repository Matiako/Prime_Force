namespace PrimeForce.Combat.Interfaces;

/// <summary>
/// Shared contract for every participant in combat (Ninja, enemies, bosses).
/// </summary>
public interface ICombatEntity
{
    string EntityId { get; }
    string DisplayName { get; }

    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }

    /// <summary>Applies pre-calculated damage after all modifiers are resolved.</summary>
    void TakeDamage(int amount);

    /// <summary>Heals by amount, capped at MaxHealth.</summary>
    void Heal(int amount);
}
