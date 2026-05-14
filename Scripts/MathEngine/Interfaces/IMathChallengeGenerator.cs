namespace PrimeForce.MathEngine.Interfaces;

/// <summary>
/// Factory that produces IMathChallenge instances based on player progress.
/// Kept separate from IMathChallenge so generation strategy can be swapped (ISP).
/// </summary>
public interface IMathChallengeGenerator
{
    /// <param name="difficultyLevel">1–10, derived from player level or enemy tier.</param>
    /// <param name="languageCode">ISO 639-1 code: "de", "en", "pl".</param>
    IMathChallenge Generate(int difficultyLevel, string languageCode);
}
