using Godot;
using System.Collections.Generic;
using Shadow;

public partial class User : Node
{
	[Export]
	public UserCharacter UserCharacter;
    [Export]
    public UserControlledShadow Shadow;
    [Export]
    public Node FreeCamera;

    public enum ControlMode {
        FreeCamera,
        UserCharacter
    }
    private List<ControlMode> _controlModes = new List<ControlMode>(){ ControlMode.FreeCamera, ControlMode.UserCharacter };
    private ControlMode _currentControlMode;
    public ControlMode CurrentControlMode {
        get {
            return _currentControlMode;
        }
        set {
            _currentControlMode = value;
            if (value == ControlMode.FreeCamera) {
                FreeCamera.Set("accept_input", true);
                UserCharacter.AcceptInput = false;
            } else if (value == ControlMode.UserCharacter) {
                FreeCamera.Set("accept_input", false);
                UserCharacter.AcceptInput = true;
                Transform3D CameraMatrix = (FreeCamera as Camera3D).GetCameraTransform();
                UserCharacter.MovementTransformMatrix = Transform3D.Identity.Rotated(Vector3.Up, CameraMatrix.basis.GetEuler().y);
            }
        }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
        CurrentControlMode = ControlMode.FreeCamera;
	}

    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed("gamepad_y")) {
            int controlModeIdx = _controlModes.IndexOf(CurrentControlMode);
            controlModeIdx += 1;
            if (controlModeIdx > _controlModes.Count - 1) {
                controlModeIdx = 0;
            }
            CurrentControlMode = _controlModes[controlModeIdx];
        }
    }
}
