using PrimeForce.MathEngine.Interfaces;

namespace PrimeForce.MathEngine.Generators;

public sealed class PrimeNumberChallenge : IMathChallenge
{
    private readonly int _correctAnswer;

    public string ChallengeId    { get; }
    public string QuestionText   { get; }
    public int    DifficultyLevel { get; }

    public PrimeNumberChallenge(
        string challengeId,
        string questionText,
        int    correctAnswer,
        int    difficultyLevel)
    {
        ChallengeId     = challengeId;
        QuestionText    = questionText;
        _correctAnswer  = correctAnswer;
        DifficultyLevel = difficultyLevel;
    }

    public bool IsAnswerCorrect(string playerAnswer)
        => int.TryParse(playerAnswer.Trim(), out var parsed) && parsed == _correctAnswer;

    /// <summary>
    /// Highest modifier of all challenge types — Boss fights only.
    /// Difficulty 1 → ×1.6 | Difficulty 10 → ×2.5
    /// </summary>
    public float GetCombatModifier() => 1.5f + DifficultyLevel * 0.1f;
}
