using Godot;
using PrimeForce.Combat.Interfaces;
using PrimeForce.Core.Events;
using PrimeForce.Core.Interfaces;
using PrimeForce.Core.Services;
using PrimeForce.Entities.Enemies;
using PrimeForce.Localization.Interfaces;

namespace PrimeForce.Entities.Player;

public partial class NinjaController : CharacterBody3D
{
    [Export] public string DisplayName   { get; set; } = "Ninja";
    [Export] public int    StartingLevel { get; set; } = 1;

    [Export] private EnemyController? TargetEnemy;

    private const float Speed         = 5f;
    private const float JumpVelocity  = 7f;
    private const float Gravity       = -20f;
    private const float RotationSpeed = 10f;

    private Vector2 _moveInput  = Vector2.Zero;
    private bool    _isBlocking = false;

    private NinjaCombatEntity        _combatEntity = null!;
    private ICombatCalculator        _calculator   = null!;
    private ILocalizationProvider    _localization = null!;
    private IEventBus                _eventBus     = null!;
    private PlayerProgressionManager _progression  = null!;
    private Camera3D                 _camera       = null!;

    public override void _Ready()
    {
        _calculator   = GameServices.Instance.Get<ICombatCalculator>();
        _localization = GameServices.Instance.Get<ILocalizationProvider>();
        _eventBus     = GameServices.Instance.Get<IEventBus>();
        _progression  = GameServices.Instance.Get<PlayerProgressionManager>();
        _camera       = GetNode<Camera3D>("../Camera3D");

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

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
            velocity.Y += Gravity * (float)delta;

        if (_moveInput.LengthSquared() > 0.01f)
        {
            // Camera-relative horizontal movement: D-Pad maps to camera's forward/right axes
            var basis      = _camera.GlobalTransform.Basis;
            var camForward = new Vector3(-basis.Z.X, 0f, -basis.Z.Z).Normalized();
            var camRight   = new Vector3(basis.X.X,  0f,  basis.X.Z).Normalized();
            var moveDir    = (camRight * _moveInput.X + camForward * -_moveInput.Y).Normalized();

            velocity.X = moveDir.X * Speed;
            velocity.Z = moveDir.Z * Speed;

            // Rotate character to face movement direction
            var targetAngle = Mathf.Atan2(moveDir.X, moveDir.Z);
            Rotation = Rotation with { Y = Mathf.LerpAngle(Rotation.Y, targetAngle, RotationSpeed * (float)delta) };
        }
        else
        {
            velocity.X = 0f;
            velocity.Z = 0f;
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    // ── Input handlers — wired via GameUiController signals in Main.tscn ──────

    public void OnMovementChanged(Vector2 direction) => _moveInput = direction;

    public void OnJumpPressed()
    {
        if (IsOnFloor())
            Velocity = Velocity with { Y = JumpVelocity };
    }

    public void OnBlockPressed()
    {
        _isBlocking = !_isBlocking;
        GD.Print($"[Ninja] Block: {(_isBlocking ? "ON" : "OFF")}");
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

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnLevelUp(PlayerLevelUpEvent e)
    {
        _combatEntity.ApplyLevelUp(e.NewLevel, e.NewMaxHealth);
        GD.Print($"[Ninja] Level up! → Level {e.NewLevel}, MaxHP {e.NewMaxHealth}");
    }
}
