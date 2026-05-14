using PrimeForce.MathEngine.Interfaces;

namespace PrimeForce.MathEngine.Generators;

public sealed class MultiplicationDivisionChallengeGenerator : IMathChallengeGenerator
{
    public IMathChallenge Generate(int difficultyLevel, string languageCode)
    {
        var (min, max) = OperandRangeFor(difficultyLevel);
        bool isMultiplication = Random.Shared.Next(2) == 0;

        int a, b, answer;
        string question;

        if (isMultiplication)
        {
            a      = Random.Shared.Next(min, max + 1);
            b      = Random.Shared.Next(min, max + 1);
            answer = a * b;
            question = $"{a} × {b} = ?";
        }
        else
        {
            // Generate result and divisor first — guarantees integer dividend, no fractions.
            int result  = Random.Shared.Next(min, max + 1);
            b           = Random.Shared.Next(2, max + 1);   // divisor ≥ 2 (÷1 is trivial)
            a           = result * b;                        // dividend
            answer      = result;
            question    = $"{a} ÷ {b} = ?";
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        return new MultiplicationDivisionChallenge(id, question, answer, difficultyLevel);
    }

    private static (int min, int max) OperandRangeFor(int difficulty) => difficulty switch
    {
        <= 3 => (2, 6),    // up to 6×6=36 — fits "up to 30" requirement for beginners
        <= 6 => (2, 10),   // up to 10×10=100
        _    => (2, 15),   // up to 15×15=225
    };
}
