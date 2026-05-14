using Godot;
using PrimeForce.Combat.Interfaces;
using PrimeForce.Core.Events;
using PrimeForce.Core.Interfaces;
using PrimeForce.Core.Services;
using PrimeForce.Entities.Enemies;
using PrimeForce.Localization.Interfaces;

namespace PrimeForce.Entities.Player;

/// <summary>
/// Thin Godot adapter. Responsibilities:
///   - Initialise NinjaCombatEntity from saved progression data.
///   - Forward attack input to ICombatCalculator.
///   - React to PlayerLevelUpEvent and update the domain entity accordingly.
/// </summary>
public partial class NinjaController : CharacterBody3D
{
    [Export] public string DisplayName   { get; set; } = "Ninja";
    [Export] public int    StartingLevel { get; set; } = 1;

    [Export] private EnemyController? TargetEnemy;

    private NinjaCombatEntity          _combatEntity = null!;
    private ICombatCalculator          _calculator   = null!;
    private ILocalizationProvider      _localization = null!;
    private IEventBus                  _eventBus     = null!;
    private PlayerProgressionManager   _progression  = null!;

    public override void _Ready()
    {
        _calculator   = GameServices.Instance.Get<ICombatCalculator>();
        _localization = GameServices.Instance.Get<ILocalizationProvider>();
        _eventBus     = GameServices.Instance.Get<IEventBus>();
        _progression  = GameServices.Instance.Get<PlayerProgressionManager>();

        // Initialise entity from persisted data — level and maxHealth from last save
        _combatEntity = new NinjaCombatEntity(
            entityId:    Name.ToString(),
            displayName: DisplayName,
            maxHealth:   _progression.Data.MaxHealth,
            level:       _progression.Data.Level);

        _eventBus.Subscribe<PlayerLevelUpEvent>(OnLevelUp);
    }

    public override void _ExitTree()
    {
        _eventBus.Unsubscribe<PlayerLevelUpEvent>(OnLevelUp);
    }

    // async void — correct pattern for Godot signal callbacks
    public async void OnAttackButtonPressed()
    {
        var target = TargetEnemy?.CombatEntity;
        if (target is null || !_combatEntity.IsAlive || !target.IsAlive)
            return;

        try
        {
            await _calculator.CalculateDamageAsync(
                attacker:        _combatEntity,
                defender:        target,
                difficultyLevel: target.DifficultyTier,
                languageCode:    _localization.CurrentLocale);
        }
        catch (OperationCanceledException)
        {
            GD.Print("[Ninja] Attack cancelled.");
        }
    }

    private void OnLevelUp(PlayerLevelUpEvent e)
    {
        _combatEntity.ApplyLevelUp(e.NewLevel, e.NewMaxHealth);
        GD.Print($"[Ninja] Level up! → Level {e.NewLevel}, MaxHP {e.NewMaxHealth}");
    }
}
