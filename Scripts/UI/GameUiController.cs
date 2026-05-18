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

    private Control _dpadArea  = null!;
    private Button  _dpadUp    = null!;
    private Button  _dpadDown  = null!;
    private Button  _dpadLeft  = null!;
    private Button  _dpadRight = null!;

    private Button _jumpButton   = null!;
    private Button _attackButton = null!;
    private Button _blockButton  = null!;

    private readonly TextureRect[] _weaponSlotIcons = new TextureRect[7];

    private IEventBus? _eventBus;

    // ── D-Pad multi-touch tracking ────────────────────────────────────────────

    // Tracks which touch/mouse input "owns" the D-Pad.
    // -1 = none active, >=0 = touch index, -2 = mouse (desktop).
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
        ConnectActionButtons();
        InitStatsFromProgression();
        SubscribeToEvents();
    }

    public override void _ExitTree()
    {
        _eventBus?.Unsubscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }

    // ── D-Pad input — area-based multi-touch ─────────────────────────────────

    public override void _Input(InputEvent ev)
    {
        var dpadRect = _dpadArea.GetGlobalRect();

        if (ev is InputEventScreenTouch touch)
        {
            if (touch.Pressed && _dpadTouchIndex < 0 && dpadRect.HasPoint(touch.Position))
            {
                _dpadTouchIndex = touch.Index;
                EmitDPadFromPosition(touch.Position, dpadRect);
            }
            else if (!touch.Pressed && touch.Index == _dpadTouchIndex)
            {
                _dpadTouchIndex = -1;
                EmitSignal(SignalName.MovementChanged, Vector2.Zero);
            }
        }
        else if (ev is InputEventScreenDrag drag && drag.Index == _dpadTouchIndex)
        {
            EmitDPadFromPosition(drag.Position, dpadRect);
        }
        else if (ev is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left)
        {
            if (mouse.Pressed && _dpadTouchIndex < 0 && dpadRect.HasPoint(mouse.Position))
            {
                _dpadTouchIndex = -2;
                EmitDPadFromPosition(mouse.Position, dpadRect);
            }
            else if (!mouse.Pressed && _dpadTouchIndex == -2)
            {
                _dpadTouchIndex = -1;
                EmitSignal(SignalName.MovementChanged, Vector2.Zero);
            }
        }
        else if (ev is InputEventMouseMotion motion && _dpadTouchIndex == -2)
        {
            if (dpadRect.HasPoint(motion.Position))
                EmitDPadFromPosition(motion.Position, dpadRect);
            else
            {
                _dpadTouchIndex = -1;
                EmitSignal(SignalName.MovementChanged, Vector2.Zero);
            }
        }
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

        const string dpad = "RootControl/DPadArea";
        _dpadArea  = GetNode<Control>(dpad);
        _dpadUp    = GetNode<Button>($"{dpad}/DPadUp");
        _dpadDown  = GetNode<Button>($"{dpad}/DPadDown");
        _dpadLeft  = GetNode<Button>($"{dpad}/DPadLeft");
        _dpadRight = GetNode<Button>($"{dpad}/DPadRight");

        const string action = "RootControl/ActionArea/HBoxContainer";
        _jumpButton   = GetNode<Button>($"{action}/JumpButton");
        _attackButton = GetNode<Button>($"{action}/AttackBlockVBox/AttackButton");
        _blockButton  = GetNode<Button>($"{action}/AttackBlockVBox/BlockButton");
    }

    private void ResolveWeaponSlotIcons()
    {
        const string hotbar = "RootControl/TopArea/TopVBox/Hotbar";
        for (int i = 0; i < _weaponSlotIcons.Length; i++)
            _weaponSlotIcons[i] = GetNode<TextureRect>($"{hotbar}/WeaponSlot{i}/WeaponIcon");
    }

    // ── Private — action button wiring ───────────────────────────────────────

    private void ConnectActionButtons()
    {
        _jumpButton.Pressed   += () => EmitSignal(SignalName.JumpPressed);
        _attackButton.Pressed += () => EmitSignal(SignalName.AttackPressed);
        _blockButton.Pressed  += () => EmitSignal(SignalName.BlockPressed);
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

    // ── Private — handlers ────────────────────────────────────────────────────

    private void EmitDPadFromPosition(Vector2 position, Rect2 dpadRect)
    {
        var offset    = position - dpadRect.GetCenter();
        var deadzone  = dpadRect.Size.X * 0.15f;
        var direction = new Vector2(
            Mathf.Abs(offset.X) > deadzone ? Mathf.Sign(offset.X) : 0f,
            Mathf.Abs(offset.Y) > deadzone ? Mathf.Sign(offset.Y) : 0f);
        EmitSignal(SignalName.MovementChanged, direction);
    }

    private void OnPlayerLevelUp(PlayerLevelUpEvent e)
    {
        UpdateLevel(e.NewLevel);
        _energyBar.MaxValue = e.NewMaxHealth;
        _energyBar.Value    = e.NewMaxHealth;
    }
}
