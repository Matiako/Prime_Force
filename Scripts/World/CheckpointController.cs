using Godot;
using PrimeForce.Core.Services;

namespace PrimeForce.World;

/// <summary>
/// Soulslike bonfire / checkpoint. Saves player progress when the Ninja enters the Area3D.
/// Delegates entirely to PlayerProgressionManager.SaveAsync() — owns no save logic.
///
/// Scene wiring (Godot editor):
///   1. Attach to an Area3D node.
///   2. Add a CollisionShape3D child with the detection volume.
///   3. Ensure the NinjaController node is in the "player" group
///      (select NinjaController → Node panel → Groups → add "player").
///   4. No signal wiring needed — BodyEntered is connected in _Ready().
/// </summary>
public partial class CheckpointController : Area3D
{
    private PlayerProgressionManager _progression = null!;

    // Prevents repeat saves when the player lingers in the area.
    // Reset externally (e.g. on player respawn) if repeated saves are needed.
    private bool _activated;

    public override void _Ready()
    {
        _progression = GameServices.Instance.Get<PlayerProgressionManager>();
        BodyEntered += OnBodyEntered;
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private async void OnBodyEntered(Node3D body)
    {
        if (_activated || !body.IsInGroup("player"))
            return;

        _activated = true;

        try
        {
            await _progression.SaveAsync();
            GD.Print($"[Checkpoint:{Name}] Progress saved.");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Checkpoint:{Name}] Save failed: {ex.Message}");
        }
    }
}
