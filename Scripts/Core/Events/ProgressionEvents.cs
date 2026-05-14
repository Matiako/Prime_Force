namespace PrimeForce.Core.Events;

/// <summary>Published by EnemyController when HP reaches zero.</summary>
public sealed record EnemyDefeatedEvent(string EnemyId, int DifficultyTier);

/// <summary>Published by PlayerProgressionManager when threshold is crossed.</summary>
public sealed record PlayerLevelUpEvent(int NewLevel, int NewMaxHealth);
