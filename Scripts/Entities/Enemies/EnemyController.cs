using Godot;
using PrimeForce.Core.Events;
using PrimeForce.Core.Interfaces;
using PrimeForce.Core.Services;

namespace PrimeForce.Entities.Enemies;

/// <summary>
/// Thin Godot adapter for EnemyCombatEntity.
/// Exposes CombatEntity so NinjaController can pass it to ICombatCalculator.
/// Reacts to DamageDealtEvent to update the health display without polling.
///
/// Scene wiring (Godot editor):
///   1. Attach to a CharacterBody3D node.
///   2. Optionally assign HealthLabel in the Inspector for on-screen HP.
///   3. Assign this node to NinjaController.TargetEnemy in the Inspector.
/// </summary>
public partial class EnemyController : CharacterBody3D
{
    [Export] public string DisplayName   { get; set; } = "Enemy";
    [Export] public int    MaxHealth     { get; set; } = 30;
    [Export] public int    DifficultyTier { get; set; } = 1;

    [Export] private Label? HealthLabel;

    private EnemyCombatEntity _combatEntity = null!;
    private IEventBus         _eventBus     = null!;

    public EnemyCombatEntity CombatEntity => _combatEntity;

    public override void _Ready()
    {
        _combatEntity = new EnemyCombatEntity(
            entityId:       Name.ToString(),
            displayName:    DisplayName,
            maxHealth:      MaxHealth,
            difficultyTier: DifficultyTier);

        _eventBus = GameServices.Instance.Get<IEventBus>();
        _eventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);

        RefreshHealthDisplay();
    }

    public override void _ExitTree()
    {
        _eventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnDamageDealt(DamageDealtEvent e)
    {
        if (e.DefenderId != _combatEntity.EntityId) return;

        RefreshHealthDisplay();

        if (!_combatEntity.IsAlive)
        {
            GD.Print($"[{DisplayName}] Defeated!");
            _eventBus.Publish(new EnemyDefeatedEvent(_combatEntity.EntityId, _combatEntity.DifficultyTier));
        }
    }

    private void RefreshHealthDisplay()
    {
        string text = $"{_combatEntity.DisplayName}: {_combatEntity.CurrentHealth}/{_combatEntity.MaxHealth} HP";
        GD.Print($"[EnemyController] {text}");

        if (HealthLabel is not null)
            HealthLabel.Text = text;
    }
}
