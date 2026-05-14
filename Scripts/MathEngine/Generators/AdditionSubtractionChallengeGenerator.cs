using PrimeForce.MathEngine.Interfaces;

namespace PrimeForce.MathEngine.Generators;

public sealed class AdditionSubtractionChallengeGenerator : IMathChallengeGenerator
{
    public IMathChallenge Generate(int difficultyLevel, string languageCode)
    {
        var (min, max) = OperandRangeFor(difficultyLevel);
        bool isAddition = Random.Shared.Next(2) == 0;

        int a = Random.Shared.Next(min, max + 1);
        int b = Random.Shared.Next(min, max + 1);

        int answer;
        string question;

        if (isAddition)
        {
            answer = a + b;
            question = $"{a} + {b} = ?";
        }
        else
        {
            // Ensure non-negative result — beginner-friendly
            if (a < b) (a, b) = (b, a);
            answer = a - b;
            question = $"{a} - {b} = ?";
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        return new AdditionSubtractionChallenge(id, question, answer, difficultyLevel);
    }

    private static (int min, int max) OperandRangeFor(int difficulty) => difficulty switch
    {
        <= 3 => (1, 10),
        <= 6 => (10, 50),
        _    => (50, 100),
    };
}
