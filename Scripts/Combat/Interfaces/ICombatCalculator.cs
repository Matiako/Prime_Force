namespace PrimeForce.Combat.Interfaces;

/// <summary>
/// Orchestrates one attack turn: generates a challenge, waits for the player's answer
/// asynchronously (non-blocking), applies damage, and fires events.
/// </summary>
public interface ICombatCalculator
{
    /// <param name="difficultyLevel">1–10, typically derived from the enemy's tier.</param>
    /// <param name="languageCode">ISO 639-1 code for challenge localisation.</param>
    /// <returns>Final damage applied to the defender.</returns>
    Task<int> CalculateDamageAsync(
        ICombatEntity attacker,
        ICombatEntity defender,
        int difficultyLevel,
        string languageCode,
        CancellationToken cancellationToken = default);
}
