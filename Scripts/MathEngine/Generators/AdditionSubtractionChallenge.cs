using PrimeForce.MathEngine.Interfaces;

namespace PrimeForce.MathEngine.Generators;

public sealed class AdditionSubtractionChallenge : IMathChallenge
{
    private readonly int _correctAnswer;

    public string ChallengeId { get; }
    public string QuestionText { get; }
    public int DifficultyLevel { get; }

    public AdditionSubtractionChallenge(
        string challengeId,
        string questionText,
        int correctAnswer,
        int difficultyLevel)
    {
        ChallengeId = challengeId;
        QuestionText = questionText;
        _correctAnswer = correctAnswer;
        DifficultyLevel = difficultyLevel;
    }

    public bool IsAnswerCorrect(string playerAnswer)
        => int.TryParse(playerAnswer.Trim(), out var parsed) && parsed == _correctAnswer;

    /// <summary>
    /// Correct-answer modifier: scales linearly from 1.1 (difficulty 1) to 2.0 (difficulty 10).
    /// The calculator uses this for a correct answer and a fixed penalty for a wrong one.
    /// </summary>
    public float GetCombatModifier() => 1.0f + DifficultyLevel * 0.1f;
}
