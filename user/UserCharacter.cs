using Godot;
using System;

public partial class UserCharacter : CharacterBody3D
{
	[Export]
	// In m/s
	public float MoveSpeed = 1;
	[Export]
	public bool AcceptInput = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (AcceptInput) {
			Vector2 moveVector = Input.GetVector("gamepad_move_left", "gamepad_move_right", "gamepad_move_forward", "gamepad_move_backward") * MoveSpeed;
			if (moveVector.Length() > MoveSpeed) moveVector = moveVector.Normalized() * MoveSpeed;
			this.Velocity = new Vector3(moveVector.x, 0, moveVector.y);
			this.MoveAndSlide();
		} else {
			this.Velocity = Vector3.Zero;
		}
	}
}
