using PrimeForce.Combat.Interfaces;

namespace PrimeForce.Entities.Player;

public sealed class NinjaCombatEntity : ICombatEntity
{
    public string EntityId { get; }
    public string DisplayName { get; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth     { get; private set; }
    public bool IsAlive      => CurrentHealth > 0;
    public int Level         { get; private set; }

    public NinjaCombatEntity(string entityId, string displayName, int maxHealth, int level = 1)
    {
        EntityId = entityId;
        DisplayName = displayName;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        Level = level;
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

    /// <summary>
    /// Applies a level-up: raises MaxHealth and heals by the difference (classic RPG reward).
    /// </summary>
    public void ApplyLevelUp(int newLevel, int newMaxHealth)
    {
        int bonus = Math.Max(0, newMaxHealth - MaxHealth);
        Level     = newLevel;
        MaxHealth = newMaxHealth;
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + bonus);
    }
}
