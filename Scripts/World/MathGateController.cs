using Godot;

namespace PrimeForce.World;

public enum MathGrade { Grade1_2, Grade3_4, Grade5_6, Grade7_8 }

public partial class MathGateController : Node3D
{
    [Export] public MathGrade CurrentGrade { get; set; } = MathGrade.Grade1_2;

    private bool   _leftIsCorrect;
    private bool   _solved = false;
    private string _questionText = "";

    private StaticBody3D _doorLeft  = null!;
    private StaticBody3D _doorRight = null!;
    private Label3D      _questionLabel    = null!;
    private Label3D      _answerLabelLeft  = null!;
    private Label3D      _answerLabelRight = null!;

    // Primes and composites used for Grade7_8 prime-check questions
    private static readonly int[] Primes =
        { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47 };

    private static readonly int[] Composites =
        { 4, 6, 8, 9, 10, 12, 14, 15, 16, 18, 20, 21, 22, 24, 25, 26, 27, 28 };

    public override void _Ready()
    {
        _doorLeft         = GetNode<StaticBody3D>("DoorLeft");
        _doorRight        = GetNode<StaticBody3D>("DoorRight");
        _questionLabel    = GetNode<Label3D>("QuestionLabel");
        _answerLabelLeft  = GetNode<Label3D>("AnswerLabelLeft");
        _answerLabelRight = GetNode<Label3D>("AnswerLabelRight");

        GetNode<Area3D>("AreaLeft").BodyEntered  += body => OnGateEntered(body, isLeft: true);
        GetNode<Area3D>("AreaRight").BodyEntered += body => OnGateEntered(body, isLeft: false);

        GenerateProblem();
    }

    private void GenerateProblem()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();

        var (question, correctText, wrongText) = CurrentGrade switch
        {
            MathGrade.Grade1_2 => GenerateAddSub(rng),
            MathGrade.Grade3_4 => GenerateMulDiv(rng),
            MathGrade.Grade5_6 => GenerateMixed(rng),
            MathGrade.Grade7_8 => GeneratePrime(rng),
            _                  => GenerateAddSub(rng),
        };

        _leftIsCorrect = rng.Randf() > 0.5f;
        _questionText  = question;

        _questionLabel.Text    = question;
        _answerLabelLeft.Text  = _leftIsCorrect ? correctText : wrongText;
        _answerLabelRight.Text = _leftIsCorrect ? wrongText   : correctText;
    }

    // ── Grade generators ───────────────────────────────────────────────────────

    // Grade 1-2: addition / subtraction, range 1-20
    private static (string, string, string) GenerateAddSub(RandomNumberGenerator rng)
    {
        int  a     = rng.RandiRange(1, 20);
        int  b     = rng.RandiRange(1, 20);
        bool isAdd = rng.Randf() > 0.5f;

        if (!isAdd && a < b) (a, b) = (b, a); // keep result positive

        int    correct = isAdd ? a + b : a - b;
        string op      = isAdd ? "+" : "-";
        return ($"{a} {op} {b} = ?", correct.ToString(), WrongAnswer(rng, correct, 3).ToString());
    }

    // Grade 3-4: multiplication / clean division from times table 1-10
    private static (string, string, string) GenerateMulDiv(RandomNumberGenerator rng)
    {
        int  a     = rng.RandiRange(1, 10);
        int  b     = rng.RandiRange(1, 10);
        bool isMul = rng.Randf() > 0.5f;

        int    correct;
        string question;

        if (isMul)
        {
            correct  = a * b;
            question = $"{a} x {b} = ?";
        }
        else
        {
            correct  = a;                      // quotient
            question = $"{a * b} / {b} = ?";  // dividend / divisor
        }

        return (question, correct.ToString(), WrongAnswer(rng, correct, 3).ToString());
    }

    // Grade 5-6: multi-digit additions/subtractions and extended 1-12 multiplication
    private static (string, string, string) GenerateMixed(RandomNumberGenerator rng)
    {
        int mode = rng.RandiRange(0, 2);

        int    a, b, correct, spread;
        string op;

        if (mode == 0)
        {
            a = rng.RandiRange(10, 99); b = rng.RandiRange(10, 99);
            correct = a + b; op = "+"; spread = 10;
        }
        else if (mode == 1)
        {
            a = rng.RandiRange(20, 99); b = rng.RandiRange(10, a);
            correct = a - b; op = "-"; spread = 10;
        }
        else
        {
            a = rng.RandiRange(1, 12); b = rng.RandiRange(1, 12);
            correct = a * b; op = "x"; spread = 3;
        }

        return ($"{a} {op} {b} = ?", correct.ToString(), WrongAnswer(rng, correct, spread).ToString());
    }

    // Grade 7-8: Prime Force — "Is X a prime number?" → TAK / NIE
    private static (string, string, string) GeneratePrime(RandomNumberGenerator rng)
    {
        bool showPrime = rng.Randf() > 0.5f;
        int  x = showPrime
            ? Primes[rng.RandiRange(0, Primes.Length - 1)]
            : Composites[rng.RandiRange(0, Composites.Length - 1)];

        string correct = showPrime ? "TAK" : "NIE";
        string wrong   = showPrime ? "NIE" : "TAK";
        return ($"Czy {x} to\nliczba pierwsza?", correct, wrong);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static int WrongAnswer(RandomNumberGenerator rng, int correct, int spread)
    {
        int offset = rng.RandiRange(1, spread) * (rng.Randf() > 0.5f ? 1 : -1);
        int wrong  = correct + offset;
        if (wrong < 0)       wrong = correct + rng.RandiRange(1, spread);
        if (wrong == correct) wrong = correct + 1;
        return wrong;
    }

    // ── Gate logic ─────────────────────────────────────────────────────────────

    private void OnGateEntered(Node3D body, bool isLeft)
    {
        if (_solved || !body.IsInGroup("player")) return;

        if (isLeft == _leftIsCorrect)
        {
            _solved = true;
            _doorLeft.QueueFree();
            _doorRight.QueueFree();
            _questionLabel.Text    = "Swietnie! :)";
            _answerLabelLeft.Text  = "";
            _answerLabelRight.Text = "";
        }
        else
        {
            _questionLabel.Text = "Sprobuj jeszcze!";
            // New problem after delay — prevents elimination guessing
            GetTree().CreateTimer(1.5).Timeout += () =>
            {
                if (!_solved) GenerateProblem();
            };
        }
    }
}
