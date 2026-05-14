using PrimeForce.Combat.Interfaces;

namespace PrimeForce.Entities.Enemies;

public sealed class EnemyCombatEntity : ICombatEntity
{
    public string EntityId { get; }
    public string DisplayName { get; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; }
    public bool IsAlive => CurrentHealth > 0;

    /// <summary>Maps to IMathChallengeGenerator.difficultyLevel (1–10).</summary>
    public int DifficultyTier { get; }

    public EnemyCombatEntity(string entityId, string displayName, int maxHealth, int difficultyTier = 1)
    {
        EntityId = entityId;
        DisplayName = displayName;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        DifficultyTier = Math.Clamp(difficultyTier, 1, 10);
    }

    public void TakeDamage(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
    }

    public void Heal(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
    }
}
