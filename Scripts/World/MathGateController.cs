using Godot;

namespace PrimeForce.World;

public partial class MathGateController : Node3D
{
    private int  _correctAnswer;
    private bool _leftIsCorrect;
    private bool _solved = false;
    private string _questionText = "";

    private StaticBody3D _doorLeft  = null!;
    private StaticBody3D _doorRight = null!;
    private Label3D      _questionLabel    = null!;
    private Label3D      _answerLabelLeft  = null!;
    private Label3D      _answerLabelRight = null!;

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

        int a = rng.RandiRange(1, 5);
        int b = rng.RandiRange(1, 5);
        _correctAnswer = a + b;

        int offset      = rng.RandiRange(1, 2) * (rng.Randf() > 0.5f ? 1 : -1);
        int wrongAnswer = Mathf.Max(1, _correctAnswer + offset);
        if (wrongAnswer == _correctAnswer) wrongAnswer = _correctAnswer + 1;

        _leftIsCorrect = rng.Randf() > 0.5f;
        _questionText  = $"{a} + {b} = ?";

        _questionLabel.Text    = _questionText;
        _answerLabelLeft.Text  = (_leftIsCorrect ? _correctAnswer : wrongAnswer).ToString();
        _answerLabelRight.Text = (_leftIsCorrect ? wrongAnswer : _correctAnswer).ToString();
    }

    private void OnGateEntered(Node3D body, bool isLeft)
    {
        if (_solved || !body.IsInGroup("player")) return;

        if (isLeft == _leftIsCorrect)
        {
            _solved = true;
            _doorLeft.QueueFree();
            _doorRight.QueueFree();
            _questionLabel.Text    = "Swietnie!  :)";
            _answerLabelLeft.Text  = "";
            _answerLabelRight.Text = "";
        }
        else
        {
            _questionLabel.Text = "Sprobuj jeszcze!";
            GetTree().CreateTimer(1.5).Timeout += () =>
            {
                if (!_solved) _questionLabel.Text = _questionText;
            };
        }
    }
}
