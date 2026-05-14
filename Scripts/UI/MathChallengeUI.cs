using Godot;
using PrimeForce.Combat.Interfaces;
using PrimeForce.Core.Events;
using PrimeForce.Core.Interfaces;
using PrimeForce.Core.Services;

namespace PrimeForce.UI;

/// <summary>
/// Thin Godot adapter: listens for ChallengeStartedEvent, shows the puzzle UI,
/// then forwards the player's answer to IChallengeAnswerReceiver.
///
/// Scene wiring (Godot editor):
///   1. Attach to a Control node (e.g. a CanvasLayer → Panel).
///   2. Assign QuestionLabel, AnswerInput, SubmitButton in the Inspector.
///   3. No signal wiring needed — event bus handles communication.
/// </summary>
public partial class MathChallengeUI : Control
{
    [Export] private Label    QuestionLabel = null!;
    [Export] private LineEdit AnswerInput   = null!;
    [Export] private Button   SubmitButton  = null!;

    private IEventBus               _eventBus       = null!;
    private IChallengeAnswerReceiver _answerReceiver = null!;

    public override void _Ready()
    {
        _eventBus       = GameServices.Instance.Get<IEventBus>();
        _answerReceiver = GameServices.Instance.Get<IChallengeAnswerReceiver>();

        _eventBus.Subscribe<ChallengeStartedEvent>(OnChallengeStarted);
        SubmitButton.Pressed += OnSubmitPressed;

        Hide();
    }

    public override void _ExitTree()
    {
        // Prevent dangling references after the node is freed
        _eventBus.Unsubscribe<ChallengeStartedEvent>(OnChallengeStarted);
        SubmitButton.Pressed -= OnSubmitPressed;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnChallengeStarted(ChallengeStartedEvent e)
    {
        QuestionLabel.Text = e.QuestionText;
        AnswerInput.Clear();
        Show();
        AnswerInput.GrabFocus();
    }

    private void OnSubmitPressed()
    {
        string answer = AnswerInput.Text.Trim();
        if (string.IsNullOrEmpty(answer)) return;

        Hide();
        _answerReceiver.SubmitAnswer(answer);
    }
}
