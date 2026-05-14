using PrimeForce.MathEngine.Interfaces;

namespace PrimeForce.MathEngine.Generators;

public sealed class MultiplicationDivisionChallenge : IMathChallenge
{
    private readonly int _correctAnswer;

    public string ChallengeId   { get; }
    public string QuestionText  { get; }
    public int    DifficultyLevel { get; }

    public MultiplicationDivisionChallenge(
        string challengeId,
        string questionText,
        int correctAnswer,
        int difficultyLevel)
    {
        ChallengeId    = challengeId;
        QuestionText   = questionText;
        _correctAnswer = correctAnswer;
        DifficultyLevel = difficultyLevel;
    }

    public bool IsAnswerCorrect(string playerAnswer)
        => int.TryParse(playerAnswer.Trim(), out var parsed) && parsed == _correctAnswer;

    /// <summary>
    /// Higher base than Add/Sub (1.2 vs 1.0) — rewards harder challenges.
    /// Difficulty 1 → ×1.3 | Difficulty 10 → ×2.2
    /// </summary>
    public float GetCombatModifier() => 1.2f + DifficultyLevel * 0.1f;
}
