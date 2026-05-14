using Godot;
using PrimeForce.Combat.Interfaces;
using PrimeForce.Combat.Systems;
using PrimeForce.Core.Events;
using PrimeForce.Core.Interfaces;
using PrimeForce.Core.Services;
using PrimeForce.MathEngine.Generators;

namespace PrimeForce.Entities.Enemies;

/// <summary>
/// Boss thin adapter. Key difference from EnemyController:
///   - Creates its OWN BasicCombatCalculator injected with PrimeNumberChallengeGenerator,
///     bypassing the LevelAwareChallengeGenerator registered in GameServices.
///   - Exposes BossCombatCalculator so NinjaController can use it for the attack turn.
///   - Reacts to phase changes and prints them to console (visual effects: Phase 7).
///
/// Scene wiring (Godot editor):
///   1. Attach to a CharacterBody3D node.
///   2. Assign this node to NinjaController.TargetBoss in the Inspector.
///   3. NinjaController update (Phase 7) will add [Export] BossController? TargetBoss
///      and call BossCombatCalculator instead of the standard ICombatCalculator.
/// </summary>
public partial class BossController : CharacterBody3D
{
    [Export] public string DisplayName           { get; set; } = "Prime Boss";
    [Export] public int    MaxHealth             { get; set; } = 300;
    [Export] public int    BaseChallengeDifficulty { get; set; } = 7;

    [Export] private Label? HealthLabel;
    [Export] private Label? PhaseLabel;

    private BossCombatEntity  _combatEntity        = null!;
    private ICombatCalculator _bossCombatCalculator = null!;
    private IEventBus         _eventBus             = null!;
    private int               _lastKnownPhase       = 1;

    /// <summary>
    /// The Ninja uses this calculator (not the one from GameServices) during Boss fights.
    /// It is pre-wired with PrimeNumberChallengeGenerator.
    /// </summary>
    public ICombatCalculator  BossCombatCalculator => _bossCombatCalculator;
    public BossCombatEntity   CombatEntity         => _combatEntity;

    public override void _Ready()
    {
        _combatEntity = new BossCombatEntity(
            entityId:               Name.ToString(),
            displayName:            DisplayName,
            maxHealth:              MaxHealth,
            baseChallengeDifficulty: BaseChallengeDifficulty);

        _eventBus = GameServices.Instance.Get<IEventBus>();

        // Boss owns a private calculator — bypasses IMathChallengeGenerator from GameServices
        var primeGenerator    = new PrimeNumberChallengeGenerator();
        _bossCombatCalculator = new BasicCombatCalculator(primeGenerator, _eventBus);

        _eventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);

        RefreshDisplay();
    }

    public override void _ExitTree()
    {
        _eventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnDamageDealt(DamageDealtEvent e)
    {
        if (e.DefenderId != _combatEntity.EntityId) return;

        RefreshDisplay();
        NotifyPhaseChange();

        if (!_combatEntity.IsAlive)
        {
            GD.Print($"[{DisplayName}] Boss defeated!");
            _eventBus.Publish(new EnemyDefeatedEvent(_combatEntity.EntityId, _combatEntity.EffectiveDifficulty));
        }
    }

    private void NotifyPhaseChange()
    {
        if (_combatEntity.CurrentPhase <= _lastKnownPhase) return;
        _lastKnownPhase = _combatEntity.CurrentPhase;
        GD.Print($"[{DisplayName}] Phase {_combatEntity.CurrentPhase}! Difficulty → {_combatEntity.EffectiveDifficulty}");
    }

    private void RefreshDisplay()
    {
        string hp    = $"{_combatEntity.CurrentHealth}/{_combatEntity.MaxHealth} HP";
        string phase = $"Phase {_combatEntity.CurrentPhase}  |  Diff {_combatEntity.EffectiveDifficulty}";
        GD.Print($"[{DisplayName}] {hp}  {phase}");
        if (HealthLabel is not null) HealthLabel.Text = hp;
        if (PhaseLabel  is not null) PhaseLabel.Text  = phase;
    }
}
