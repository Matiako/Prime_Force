using PrimeForce.MathEngine.Interfaces;

namespace PrimeForce.MathEngine.Generators;

/// <summary>
/// Generates two types of prime-number challenges:
///   Identification (difficulty 1–4): "Is [n] prime? (1=yes / 0=no)"
///   Sequence       (difficulty 5–10): "Next prime after [n]?"
/// Used exclusively by BossController — not part of LevelAwareChallengeGenerator.
/// </summary>
public sealed class PrimeNumberChallengeGenerator : IMathChallengeGenerator
{
    public IMathChallenge Generate(int difficultyLevel, string languageCode)
    {
        var (min, max) = RangeFor(difficultyLevel);
        var id = Guid.NewGuid().ToString("N")[..8];

        return difficultyLevel <= 4
            ? GenerateIdentification(id, min, max, difficultyLevel)
            : GenerateSequence(id, min, max, difficultyLevel);
    }

    // ── Challenge builders ────────────────────────────────────────────────────

    private static IMathChallenge GenerateIdentification(
        string id, int min, int max, int difficulty)
    {
        int n        = Random.Shared.Next(min, max + 1);
        bool isPrime = IsPrime(n);
        int answer   = isPrime ? 1 : 0;
        string question = $"Is {n} a prime number? (1 = yes / 0 = no)";
        return new PrimeNumberChallenge(id, question, answer, difficulty);
    }

    private static IMathChallenge GenerateSequence(
        string id, int min, int max, int difficulty)
    {
        // Pick a starting point with at least one prime ahead in range
        int n    = Random.Shared.Next(min, max - 5);
        int next = NextPrime(n + 1);
        string question = $"Next prime after {n}?";
        return new PrimeNumberChallenge(id, question, next, difficulty);
    }

    // ── Prime utilities ───────────────────────────────────────────────────────

    internal static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        for (int i = 3; i * i <= n; i += 2)
            if (n % i == 0) return false;
        return true;
    }

    internal static int NextPrime(int from)
    {
        int candidate = from < 2 ? 2 : from;
        while (!IsPrime(candidate)) candidate++;
        return candidate;
    }

    private static (int min, int max) RangeFor(int difficulty) => difficulty switch
    {
        <= 3 => (2,  20),
        <= 6 => (2,  50),
        _    => (2, 100),
    };
}
