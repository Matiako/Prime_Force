using PrimeForce.Combat.Interfaces;
using PrimeForce.Core.Events;
using PrimeForce.Core.Interfaces;
using PrimeForce.MathEngine.Interfaces;

namespace PrimeForce.Combat.Systems;

/// <summary>
/// Async attack turn:
///   1. Generate challenge → publish ChallengeStartedEvent (UI shows puzzle)
///   2. Await player answer via SubmitAnswer() — main thread never blocks
///   3. Apply damage to defender
///   4. Publish ChallengeAnsweredEvent + DamageDealtEvent
/// </summary>
public sealed class BasicCombatCalculator : ICombatCalculator, IChallengeAnswerReceiver
{
    private const int BaseDamage = 10;
    private const float IncorrectAnswerModifier = 0.5f;

    private readonly IMathChallengeGenerator _generator;
    private readonly IEventBus _eventBus;

    // Holds the pending player answer; null when no challenge is active.
    private TaskCompletionSource<string>? _pendingAnswer;

    public BasicCombatCalculator(IMathChallengeGenerator generator, IEventBus eventBus)
    {
        _generator = generator;
        _eventBus = eventBus;
    }

    public async Task<int> CalculateDamageAsync(
        ICombatEntity attacker,
        ICombatEntity defender,
        int difficultyLevel,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        if (_pendingAnswer is not null)
            throw new InvalidOperationException("A challenge is already in progress.");

        var challenge = _generator.Generate(difficultyLevel, languageCode);

        _eventBus.Publish(new ChallengeStartedEvent(
            challenge.ChallengeId,
            challenge.QuestionText,
            challenge.DifficultyLevel));

        // No RunContinuationsAsynchronously: continuation runs synchronously on the thread
        // that calls TrySetResult (Godot main thread via SubmitAnswer), keeping all
        // event-bus callbacks and TakeDamage on the main thread — required by Godot API.
        _pendingAnswer = new TaskCompletionSource<string>();

        cancellationToken.Register(() => _pendingAnswer.TrySetCanceled(cancellationToken));

        string playerAnswer;
        try
        {
            playerAnswer = await _pendingAnswer.Task.ConfigureAwait(false);
        }
        finally
        {
            _pendingAnswer = null;
        }

        bool correct = challenge.IsAnswerCorrect(playerAnswer);
        float modifier = correct ? challenge.GetCombatModifier() : IncorrectAnswerModifier;
        int finalDamage = (int)Math.Round(BaseDamage * modifier);

        defender.TakeDamage(finalDamage);

        _eventBus.Publish(new ChallengeAnsweredEvent(
            challenge.ChallengeId,
            attacker.EntityId,
            correct,
            modifier));

        _eventBus.Publish(new DamageDealtEvent(
            attacker.EntityId,
            defender.EntityId,
            finalDamage,
            correct));

        return finalDamage;
    }

    // Called by UI input handler after the player submits an answer.
    public void SubmitAnswer(string playerAnswer)
        => _pendingAnswer?.TrySetResult(playerAnswer);
}
