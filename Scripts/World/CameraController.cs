using Godot;

namespace PrimeForce.World;

public partial class CameraController : Camera3D
{
    [Export] public float FollowSpeed { get; set; } = 6f;

    private Node3D _target = null!;

    public override void _Ready()
    {
        _target = GetNode<Node3D>("../Ninja");
    }

    public override void _Process(double delta)
    {
        var x = Mathf.Lerp(GlobalPosition.X, _target.GlobalPosition.X,
                           Mathf.Min(FollowSpeed * (float)delta, 1f));
        GlobalPosition = GlobalPosition with { X = x };
    }
}
