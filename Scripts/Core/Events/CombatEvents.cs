namespace PrimeForce.Core.Events;

public sealed record DamageDealtEvent(
    string AttackerId,
    string DefenderId,
    int FinalDamage,
    bool WasChallengeCorrect
);
