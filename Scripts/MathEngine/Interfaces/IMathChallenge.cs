namespace PrimeForce.MathEngine.Interfaces;

/// <summary>
/// Represents a single math puzzle presented to the player during combat or exploration.
/// </summary>
public interface IMathChallenge
{
    /// <summary>Unique identifier — used for save/restore and analytics.</summary>
    string ChallengeId { get; }

    /// <summary>Human-readable question string (already localised by the provider).</summary>
    string QuestionText { get; }

    /// <summary>Difficulty 1–10; drives both enemy damage modifier and XP reward.</summary>
    int DifficultyLevel { get; }

    /// <summary>Validates the player's submitted answer.</summary>
    bool IsAnswerCorrect(string playerAnswer);

    /// <summary>
    /// Calculates the combat modifier this challenge grants on a correct answer.
    /// Positive values buff the player; negative values could represent failed challenges.
    /// </summary>
    float GetCombatModifier();
}
