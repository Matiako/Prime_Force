using PrimeForce.MathEngine.Interfaces;

namespace PrimeForce.MathEngine.Generators;

/// <summary>
/// Composite generator that delegates to one of two strategies based on player level.
/// Depends on a Func&lt;int&gt; (not on PlayerProgressionManager directly) — pure DIP.
///
/// Registered as IMathChallengeGenerator in GameServices.Bootstrap():
///   new LevelAwareChallengeGenerator(addSub, mulDiv, () => progression.Data.Level, threshold: 5)
/// </summary>
public sealed class LevelAwareChallengeGenerator : IMathChallengeGenerator
{
    private readonly IMathChallengeGenerator _lowLevelGenerator;
    private readonly IMathChallengeGenerator _highLevelGenerator;
    private readonly Func<int>               _getCurrentLevel;
    private readonly int                     _threshold;

    public LevelAwareChallengeGenerator(
        IMathChallengeGenerator lowLevelGenerator,
        IMathChallengeGenerator highLevelGenerator,
        Func<int>               getCurrentLevel,
        int                     threshold = 5)
    {
        _lowLevelGenerator  = lowLevelGenerator;
        _highLevelGenerator = highLevelGenerator;
        _getCurrentLevel    = getCurrentLevel;
        _threshold          = threshold;
    }

    public IMathChallenge Generate(int difficultyLevel, string languageCode)
    {
        var generator = _getCurrentLevel() <= _threshold
            ? _lowLevelGenerator
            : _highLevelGenerator;

        return generator.Generate(difficultyLevel, languageCode);
    }
}
