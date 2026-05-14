namespace PrimeForce.Core.Events;

/// <summary>Published when the player submits an answer.</summary>
public sealed record ChallengeAnsweredEvent(
    string ChallengeId,
    string PlayerId,
    bool WasCorrect,
    float CombatModifier
);

/// <summary>Published when a new challenge is presented (UI listens to this).</summary>
public sealed record ChallengeStartedEvent(
    string ChallengeId,
    string QuestionText,
    int DifficultyLevel
);
