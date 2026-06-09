using Godot;
using PrimeForce.Entities.Player;

namespace PrimeForce.World;

public partial class CameraController : Camera3D
{
    [Export] public float FollowSpeed { get; set; } = 8f;
    [Export] public float SwingSpeed  { get; set; } = 4f;
    [Export] public float Distance    { get; set; } = 10f;
    [Export] public float Height      { get; set; } = 5f;

    private NinjaController _ninja = null!;
    private float           _yaw   = 0f;

    public override void _Ready()
    {
        _ninja = GetNode<NinjaController>("../Ninja");
        _yaw   = _ninja.Rotation.Y;
    }

    public override void _Process(double delta)
    {
        // Swing behind the character whenever it moves or rotates in place
        if (_ninja.IsMoving)
            _yaw = Mathf.LerpAngle(_yaw, _ninja.Rotation.Y, SwingSpeed * (float)delta);

        var offset = new Vector3(Mathf.Sin(_yaw) * Distance, Height, Mathf.Cos(_yaw) * Distance);
        GlobalPosition = GlobalPosition.Lerp(
            _ninja.GlobalPosition + offset,
            Mathf.Min(FollowSpeed * (float)delta, 1f));

        LookAt(_ninja.GlobalPosition + Vector3.Up * 0.85f, Vector3.Up);
    }
}
