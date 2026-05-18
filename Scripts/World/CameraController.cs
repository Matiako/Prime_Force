using Godot;

namespace PrimeForce.World;

public partial class CameraController : Camera3D
{
    [Export] public float FollowSpeed  { get; set; } = 6f;
    [Export] public float HeightOffset { get; set; } = 4f;
    [Export] public float ZOffset      { get; set; } = 12f;

    private Node3D _target = null!;

    public override void _Ready()
    {
        _target = GetNode<Node3D>("../Ninja");
    }

    public override void _Process(double delta)
    {
        var desired = _target.GlobalPosition + new Vector3(0, HeightOffset, ZOffset);
        GlobalPosition = GlobalPosition.Lerp(desired, Mathf.Min(FollowSpeed * (float)delta, 1f));
        LookAt(_target.GlobalPosition + Vector3.Up, Vector3.Up);
    }
}
