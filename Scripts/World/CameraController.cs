using Godot;
using PrimeForce.Entities.Player;

namespace PrimeForce.World;

public partial class CameraController : Camera3D
{
    [Export] public float   FollowSpeed { get; set; } = 5f;
    [Export] public Vector3 Offset      { get; set; } = new Vector3(0f, 12f, 9f);

    private NinjaController _ninja = null!;

    public override void _Ready()
    {
        _ninja = GetNode<NinjaController>("../Ninja");
        // Snap to correct position on first frame — no lerp delay at start
        GlobalPosition = _ninja.GlobalPosition + Offset;
        LookAt(_ninja.GlobalPosition, Vector3.Up);
    }

    public override void _Process(double delta)
    {
        // Smooth follow — no rotation, no yaw, no swing
        GlobalPosition = GlobalPosition.Lerp(
            _ninja.GlobalPosition + Offset,
            Mathf.Min(FollowSpeed * (float)delta, 1f));

        LookAt(_ninja.GlobalPosition, Vector3.Up);
    }
}
