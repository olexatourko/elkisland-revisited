using Godot;
using System;

public partial class PathFollowerDriver : PathFollow3D
{
	[Export]
	public float Speed = 1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		Progress += (float) delta * Speed;
	}
}
