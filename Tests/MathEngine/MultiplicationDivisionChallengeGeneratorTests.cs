using PrimeForce.MathEngine.Generators;
using Xunit;

namespace PrimeForce.Tests.MathEngine;

/// <summary>
/// Exemplary test class for MultiplicationDivisionChallengeGenerator.
/// All domain classes are pure C# — no Godot runtime required.
/// </summary>
public sealed class MultiplicationDivisionChallengeGeneratorTests
{
    private readonly MultiplicationDivisionChallengeGenerator _sut = new();

    // ── Smoke tests ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(10)]
    public void Generate_AnyDifficulty_ReturnsPopulatedChallenge(int difficulty)
    {
        var challenge = _sut.Generate(difficulty, "en");

        Assert.NotNull(challenge);
        Assert.NotEmpty(challenge.ChallengeId);
        Assert.NotEmpty(challenge.QuestionText);
        Assert.Equal(difficulty, challenge.DifficultyLevel);
        Assert.True(challenge.QuestionText.EndsWith("= ?"));
    }

    [Fact]
    public void Generate_ChallengeIds_AreUnique()
    {
        var ids = Enumerable.Range(0, 200)
            .Select(_ => _sut.Generate(1, "en").ChallengeId)
            .ToHashSet();

        Assert.Equal(200, ids.Count);
    }

    // ── Division — no fractions (core correctness invariant) ─────────────────

    [Fact]
    public void Generate_Division_ResultIsAlwaysInteger()
    {
        int divisionChecks = 0;

        for (int i = 0; i < 2_000; i++)
        {
            var c = _sut.Generate(5, "en");
            if (!c.QuestionText.Contains('÷')) continue;

            divisionChecks++;
            (int dividend, int divisor) = ParseDivision(c.QuestionText);

            Assert.Equal(0, dividend % divisor);
        }

        // Ensure we actually tested enough division cases
        Assert.True(divisionChecks >= 100,
            $"Too few division cases generated ({divisionChecks}/2000). Check Random distribution.");
    }

    [Fact]
    public void Generate_Division_DivisorIsNeverOne()
    {
        for (int i = 0; i < 2_000; i++)
        {
            var c = _sut.Generate(3, "en");
            if (!c.QuestionText.Contains('÷')) continue;

            (_, int divisor) = ParseDivision(c.QuestionText);
            Assert.True(divisor >= 2, $"Trivial divisor of 1 found in: {c.QuestionText}");
        }
    }

    // ── Operand ranges ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 2, 6)]
    [InlineData(2, 2, 6)]
    [InlineData(4, 2, 10)]
    [InlineData(5, 2, 10)]
    [InlineData(7, 2, 15)]
    [InlineData(10, 2, 15)]
    public void Generate_Multiplication_OperandsWithinExpectedRange(
        int difficulty, int minOperand, int maxOperand)
    {
        for (int i = 0; i < 500; i++)
        {
            var c = _sut.Generate(difficulty, "en");
            if (!c.QuestionText.Contains('×')) continue;

            (int a, int b) = ParseMultiplication(c.QuestionText);

            Assert.InRange(a, minOperand, maxOperand);
            Assert.InRange(b, minOperand, maxOperand);
        }
    }

    // ── Answer validation ─────────────────────────────────────────────────────

    [Fact]
    public void IsAnswerCorrect_CorrectMultiplicationAnswer_ReturnsTrue()
    {
        for (int i = 0; i < 500; i++)
        {
            var c = _sut.Generate(3, "en");
            if (!c.QuestionText.Contains('×')) continue;

            (int a, int b) = ParseMultiplication(c.QuestionText);
            Assert.True(c.IsAnswerCorrect((a * b).ToString()),
                $"Correct answer rejected for: {c.QuestionText}");
            return;
        }
        Assert.Fail("No multiplication challenge generated in 500 attempts.");
    }

    [Fact]
    public void IsAnswerCorrect_CorrectDivisionAnswer_ReturnsTrue()
    {
        for (int i = 0; i < 500; i++)
        {
            var c = _sut.Generate(4, "en");
            if (!c.QuestionText.Contains('÷')) continue;

            (int dividend, int divisor) = ParseDivision(c.QuestionText);
            int expected = dividend / divisor;

            Assert.True(c.IsAnswerCorrect(expected.ToString()),
                $"Correct answer rejected for: {c.QuestionText}");
            return;
        }
        Assert.Fail("No division challenge generated in 500 attempts.");
    }

    [Fact]
    public void IsAnswerCorrect_WrongAnswer_ReturnsFalse()
    {
        var challenge = _sut.Generate(1, "en");
        Assert.False(challenge.IsAnswerCorrect("999999"));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1.5")]
    public void IsAnswerCorrect_NonIntegerInput_ReturnsFalse(string input)
    {
        var challenge = _sut.Generate(1, "en");
        Assert.False(challenge.IsAnswerCorrect(input));
    }

    // ── Combat modifier ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 1.3f)]
    [InlineData(5, 1.7f)]
    [InlineData(10, 2.2f)]
    public void GetCombatModifier_ScalesWithDifficulty(int difficulty, float expectedModifier)
    {
        var challenge = _sut.Generate(difficulty, "en");
        Assert.Equal(expectedModifier, challenge.GetCombatModifier(), precision: 4);
    }

    [Fact]
    public void GetCombatModifier_IsHigherThanAdditionSubtractionAtSameDifficulty()
    {
        var mulDivChallenge = _sut.Generate(5, "en");
        var addSubChallenge = new AdditionSubtractionChallengeGenerator().Generate(5, "en");

        Assert.True(mulDivChallenge.GetCombatModifier() > addSubChallenge.GetCombatModifier(),
            "Mul/Div modifier should exceed Add/Sub modifier at the same difficulty.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (int a, int b) ParseMultiplication(string questionText)
    {
        var parts = questionText.Replace(" = ?", "").Split(" × ");
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static (int dividend, int divisor) ParseDivision(string questionText)
    {
        var parts = questionText.Replace(" = ?", "").Split(" ÷ ");
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }
}
