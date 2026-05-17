using Godot;

namespace PrimeForce.UI;

/// <summary>
/// Single-responsibility HUD controller.
/// Owns all UI node references, exposes stat-update methods,
/// and emits Godot signals for game logic to consume.
///
/// Scene wiring (Inspector):
///   Assign every [Export] field.
///   WeaponSlotIcons are resolved automatically by node path on _Ready.
/// </summary>
public partial class GameUiController : CanvasLayer
{
    // ── Stats ─────────────────────────────────────────────────────────────────

    [Export] private Label       GoldLabel      = null!;
    [Export] private Label       GeldLabel      = null!;
    [Export] private Label       MetallLabel    = null!;
    [Export] private Label       KristalleLabel = null!;
    [Export] private Label       LevelLabel     = null!;
    [Export] private ProgressBar EnergyBar      = null!;

    // ── D-Pad ─────────────────────────────────────────────────────────────────

    [Export] private Button DPadUp    = null!;
    [Export] private Button DPadDown  = null!;
    [Export] private Button DPadLeft  = null!;
    [Export] private Button DPadRight = null!;

    // ── Action buttons ────────────────────────────────────────────────────────

    [Export] private Button JumpButton   = null!;
    [Export] private Button AttackButton = null!;
    [Export] private Button BlockButton  = null!;

    // Resolved by path — no Inspector wiring needed for slots
    private readonly TextureRect[] _weaponSlotIcons = new TextureRect[7];

    // ── Output signals (consumed by game logic, never by this script) ─────────

    [Signal] public delegate void MovementChangedEventHandler(Vector2 direction);
    [Signal] public delegate void JumpPressedEventHandler();
    [Signal] public delegate void AttackPressedEventHandler();
    [Signal] public delegate void BlockPressedEventHandler();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void _Ready()
    {
        ResolveWeaponSlotIcons();
        ConnectDPad();
        ConnectActionButtons();
    }

    // ── Public API — resource stats ───────────────────────────────────────────

    public void UpdateGold(int amount)      => GoldLabel.Text      = $"Gold: {amount}";
    public void UpdateGeld(int amount)      => GeldLabel.Text      = $"Geld: {amount}";
    public void UpdateMetall(int amount)    => MetallLabel.Text    = $"Metall: {amount}";
    public void UpdateKristalle(int amount) => KristalleLabel.Text = $"Kristalle: {amount}";
    public void UpdateLevel(int level)      => LevelLabel.Text     = $"Level: {level}";

    /// <param name="value">0.0 – 100.0</param>
    public void UpdateEnergy(float value)   => EnergyBar.Value     = value;

    // ── Public API — hotbar ───────────────────────────────────────────────────

    /// <summary>Sets the icon texture for the given hotbar slot (0–6).</summary>
    public void SetWeaponSlotIcon(int slot, Texture2D? icon)
    {
        if ((uint)slot >= (uint)_weaponSlotIcons.Length) return;
        _weaponSlotIcons[slot].Texture = icon;
    }

    // ── Private — scene wiring ────────────────────────────────────────────────

    private void ResolveWeaponSlotIcons()
    {
        const string hotbarPath = "RootControl/TopArea/TopVBox/Hotbar";
        for (int i = 0; i < _weaponSlotIcons.Length; i++)
            _weaponSlotIcons[i] = GetNode<TextureRect>($"{hotbarPath}/WeaponSlot{i}/WeaponIcon");
    }

    private void ConnectDPad()
    {
        DPadUp.ButtonDown    += OnDPadChanged;
        DPadDown.ButtonDown  += OnDPadChanged;
        DPadLeft.ButtonDown  += OnDPadChanged;
        DPadRight.ButtonDown += OnDPadChanged;
        DPadUp.ButtonUp      += OnDPadChanged;
        DPadDown.ButtonUp    += OnDPadChanged;
        DPadLeft.ButtonUp    += OnDPadChanged;
        DPadRight.ButtonUp   += OnDPadChanged;
    }

    private void ConnectActionButtons()
    {
        JumpButton.Pressed   += () => EmitSignal("JumpPressed");
        AttackButton.Pressed += () => EmitSignal("AttackPressed");
        BlockButton.Pressed  += () => EmitSignal("BlockPressed");
    }

    private void OnDPadChanged()
    {
        var direction = new Vector2(
            (DPadRight.ButtonPressed ? 1f : 0f) - (DPadLeft.ButtonPressed ? 1f : 0f),
            (DPadDown.ButtonPressed  ? 1f : 0f) - (DPadUp.ButtonPressed   ? 1f : 0f)
        );
        EmitSignal("MovementChanged", direction);
    }
}
