using PrimeForce.Combat.Interfaces;

namespace PrimeForce.Entities.Enemies;

/// <summary>
/// Domain entity for Boss fights. Separated from EnemyCombatEntity (SRP):
///   - Tracks combat phases (HP thresholds change challenge difficulty mid-fight)
///   - Exposes ChallengeDifficulty that scales with phase
/// </summary>
public sealed class BossCombatEntity : ICombatEntity
{
    private static readonly float[] PhaseThresholds = { 0.66f, 0.33f }; // 66% and 33% HP

    public string EntityId     { get; }
    public string DisplayName  { get; }
    public int    CurrentHealth { get; private set; }
    public int    MaxHealth     { get; }
    public bool   IsAlive       => CurrentHealth > 0;

    /// <summary>Phase 1 = normal, 2 = enraged, 3 = desperate. Advances on HP thresholds.</summary>
    public int CurrentPhase { get; private set; } = 1;

    /// <summary>Base difficulty set in the editor; scales up per phase.</summary>
    public int BaseChallengeDifficulty { get; }

    /// <summary>Effective difficulty passed to PrimeNumberChallengeGenerator.</summary>
    public int EffectiveDifficulty =>
        Math.Min(10, BaseChallengeDifficulty + (CurrentPhase - 1) * 2);

    public BossCombatEntity(
        string entityId,
        string displayName,
        int    maxHealth,
        int    baseChallengeDifficulty = 7)
    {
        EntityId                 = entityId;
        DisplayName              = displayName;
        MaxHealth                = maxHealth;
        CurrentHealth            = maxHealth;
        BaseChallengeDifficulty  = Math.Clamp(baseChallengeDifficulty, 1, 10);
    }

    public void TakeDamage(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        AdvancePhaseIfNeeded();
    }

    public void Heal(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void AdvancePhaseIfNeeded()
    {
        float ratio = (float)CurrentHealth / MaxHealth;
        int expectedPhase = 1;
        foreach (float threshold in PhaseThresholds)
            if (ratio < threshold) expectedPhase++;

        if (expectedPhase > CurrentPhase)
            CurrentPhase = expectedPhase;
    }
}
