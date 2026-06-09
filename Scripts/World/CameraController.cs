using Godot;

namespace PrimeForce.World;

public partial class CameraController : Camera3D
{
    [Export] public float FollowSpeed { get; set; } = 8f;
    [Export] public float SwingSpeed  { get; set; } = 4f;
    [Export] public float Distance    { get; set; } = 10f;
    [Export] public float Height      { get; set; } = 5f;

    private Node3D _target = null!;
    private float  _yaw    = 0f;

    public override void _Ready()
    {
        _target = GetNode<Node3D>("../Ninja");
        _yaw    = -_target.Rotation.Y;
    }

    public override void _Process(double delta)
    {
        // Swing yaw to stay behind the player's facing direction
        _yaw = Mathf.LerpAngle(_yaw, -_target.Rotation.Y, SwingSpeed * (float)delta);

        var offset = new Vector3(Mathf.Sin(_yaw) * Distance, Height, Mathf.Cos(_yaw) * Distance);
        GlobalPosition = GlobalPosition.Lerp(
            _target.GlobalPosition + offset,
            Mathf.Min(FollowSpeed * (float)delta, 1f));

        LookAt(_target.GlobalPosition + Vector3.Up * 0.85f, Vector3.Up);
    }
}
