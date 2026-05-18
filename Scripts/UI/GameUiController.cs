using Godot;
using PrimeForce.Core.Events;
using PrimeForce.Core.Interfaces;
using PrimeForce.Core.Services;

namespace PrimeForce.UI;

public partial class GameUiController : CanvasLayer
{
    // ── Cached node references ────────────────────────────────────────────────

    private Label       _goldLabel      = null!;
    private Label       _geldLabel      = null!;
    private Label       _metallLabel    = null!;
    private Label       _kristalleLabel = null!;
    private Label       _levelLabel     = null!;
    private ProgressBar _energyBar      = null!;

    private Control _dpadArea    = null!;
    private Button  _jumpButton  = null!;
    private Button  _attackButton = null!;
    private Button  _blockButton  = null!;

    private readonly TextureRect[] _weaponSlotIcons = new TextureRect[7];

    private IEventBus? _eventBus;

    // Tracks which touch/mouse owns the D-Pad. -1 = none, >=0 = touch index, -2 = mouse.
    private int _dpadTouchIndex = -1;

    // ── Output signals ────────────────────────────────────────────────────────

    [Signal] public delegate void MovementChangedEventHandler(Vector2 direction);
    [Signal] public delegate void JumpPressedEventHandler();
    [Signal] public delegate void AttackPressedEventHandler();
    [Signal] public delegate void BlockPressedEventHandler();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void _Ready()
    {
        ResolveNodes();
        ResolveWeaponSlotIcons();
        InitStatsFromProgression();
        SubscribeToEvents();
    }

    public override void _ExitTree()
    {
        _eventBus?.Unsubscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }

    // ── Input — all buttons handled here so every touch index is independent ──

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventScreenTouch touch)
            HandleScreenTouch(touch);
        else if (ev is InputEventScreenDrag drag)
            HandleScreenDrag(drag);
        else if (ev is InputEventMouseButton mouse)
            HandleMouseButton(mouse);
        else if (ev is InputEventMouseMotion motion && _dpadTouchIndex == -2)
            HandleMouseMotion(motion);
    }

    // ── Public API — stats bar ────────────────────────────────────────────────

    public void UpdateGold(int amount)      => _goldLabel.Text      = $"Gold: {amount}";
    public void UpdateGeld(int amount)      => _geldLabel.Text      = $"Geld: {amount}";
    public void UpdateMetall(int amount)    => _metallLabel.Text    = $"Metall: {amount}";
    public void UpdateKristalle(int amount) => _kristalleLabel.Text = $"Kristalle: {amount}";
    public void UpdateLevel(int level)      => _levelLabel.Text     = $"Level: {level}";
    public void UpdateEnergy(float value)   => _energyBar.Value     = value;

    // ── Public API — hotbar ───────────────────────────────────────────────────

    public void SetWeaponSlotIcon(int slot, Texture2D? icon)
    {
        if ((uint)slot >= (uint)_weaponSlotIcons.Length) return;
        _weaponSlotIcons[slot].Texture = icon;
    }

    // ── Private — node resolution ─────────────────────────────────────────────

    private void ResolveNodes()
    {
        const string stats = "RootControl/TopArea/TopVBox/StatsBar";
        _goldLabel      = GetNode<Label>($"{stats}/GoldLabel");
        _geldLabel      = GetNode<Label>($"{stats}/GeldLabel");
        _metallLabel    = GetNode<Label>($"{stats}/MetallLabel");
        _kristalleLabel = GetNode<Label>($"{stats}/KristalleLabel");
        _levelLabel     = GetNode<Label>($"{stats}/LevelLabel");
        _energyBar      = GetNode<ProgressBar>($"{stats}/EnergyBar");

        _dpadArea    = GetNode<Control>("RootControl/DPadArea");
        _jumpButton  = GetNode<Button>("RootControl/ActionArea/HBoxContainer/JumpButton");
        _attackButton = GetNode<Button>("RootControl/ActionArea/HBoxContainer/AttackBlockVBox/AttackButton");
        _blockButton  = GetNode<Button>("RootControl/ActionArea/HBoxContainer/AttackBlockVBox/BlockButton");
    }

    private void ResolveWeaponSlotIcons()
    {
        const string hotbar = "RootControl/TopArea/TopVBox/Hotbar";
        for (int i = 0; i < _weaponSlotIcons.Length; i++)
            _weaponSlotIcons[i] = GetNode<TextureRect>($"{hotbar}/WeaponSlot{i}/WeaponIcon");
    }

    // ── Private — input handlers ──────────────────────────────────────────────

    private void HandleScreenTouch(InputEventScreenTouch ev)
    {
        if (ev.Pressed)
        {
            // Only one finger can own the D-Pad at a time; others go to action buttons.
            if (_dpadTouchIndex < 0 && _dpadArea.GetGlobalRect().HasPoint(ev.Position))
            {
                _dpadTouchIndex = ev.Index;
                EmitDPadFromPosition(ev.Position);
                return;
            }
            EmitActionSignal(ev.Position);
        }
        else if (ev.Index == _dpadTouchIndex)
        {
            _dpadTouchIndex = -1;
            EmitSignal(SignalName.MovementChanged, Vector2.Zero);
        }
    }

    private void HandleScreenDrag(InputEventScreenDrag ev)
    {
        if (ev.Index == _dpadTouchIndex)
            EmitDPadFromPosition(ev.Position);
    }

    private void HandleMouseButton(InputEventMouseButton ev)
    {
        if (ev.ButtonIndex != MouseButton.Left) return;

        if (ev.Pressed)
        {
            if (_dpadTouchIndex < 0 && _dpadArea.GetGlobalRect().HasPoint(ev.Position))
            {
                _dpadTouchIndex = -2;
                EmitDPadFromPosition(ev.Position);
                return;
            }
            EmitActionSignal(ev.Position);
        }
        else if (_dpadTouchIndex == -2)
        {
            _dpadTouchIndex = -1;
            EmitSignal(SignalName.MovementChanged, Vector2.Zero);
        }
    }

    private void HandleMouseMotion(InputEventMouseMotion ev)
    {
        if (_dpadArea.GetGlobalRect().HasPoint(ev.Position))
            EmitDPadFromPosition(ev.Position);
        else
        {
            _dpadTouchIndex = -1;
            EmitSignal(SignalName.MovementChanged, Vector2.Zero);
        }
    }

    private void EmitDPadFromPosition(Vector2 position)
    {
        var rect     = _dpadArea.GetGlobalRect();
        var offset   = position - rect.GetCenter();
        var deadzone = rect.Size.X * 0.15f;
        EmitSignal(SignalName.MovementChanged, new Vector2(
            Mathf.Abs(offset.X) > deadzone ? Mathf.Sign(offset.X) : 0f,
            Mathf.Abs(offset.Y) > deadzone ? Mathf.Sign(offset.Y) : 0f));
    }

    private void EmitActionSignal(Vector2 position)
    {
        if (_jumpButton.GetGlobalRect().HasPoint(position))
            EmitSignal(SignalName.JumpPressed);
        else if (_attackButton.GetGlobalRect().HasPoint(position))
            EmitSignal(SignalName.AttackPressed);
        else if (_blockButton.GetGlobalRect().HasPoint(position))
            EmitSignal(SignalName.BlockPressed);
    }

    // ── Private — progression init & event subscription ───────────────────────

    private void InitStatsFromProgression()
    {
        if (GameServices.Instance is null) return;
        if (!GameServices.Instance.IsRegistered<PlayerProgressionManager>()) return;

        var data = GameServices.Instance.Get<PlayerProgressionManager>().Data;
        UpdateLevel(data.Level);
        _energyBar.MaxValue = data.MaxHealth;
        _energyBar.Value    = data.MaxHealth;
    }

    private void SubscribeToEvents()
    {
        if (GameServices.Instance is null) return;
        if (!GameServices.Instance.IsRegistered<IEventBus>()) return;

        _eventBus = GameServices.Instance.Get<IEventBus>();
        _eventBus.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }

    private void OnPlayerLevelUp(PlayerLevelUpEvent e)
    {
        UpdateLevel(e.NewLevel);
        _energyBar.MaxValue = e.NewMaxHealth;
        _energyBar.Value    = e.NewMaxHealth;
    }
}
