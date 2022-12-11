# Based off of https://github.com/adamviola/simple-free-look-camera
class_name FreeLookCamera extends Camera3D

# Modifier keys' speed multiplier
const SHIFT_MULTIPLIER = 2.5
const ALT_MULTIPLIER = 1.0 / SHIFT_MULTIPLIER

@export_range(0.0, 1.0) var sensitivity = 0.25
@export var use_mouse = false

# Mouse state
var _mouse_position = Vector2(0.0, 0.0)
var _total_pitch = 0.0

# Movement state
var _direction = Vector3(0.0, 0.0, 0.0)
var _velocity = Vector3(0.0, 0.0, 0.0)
var _acceleration = 30
var _deceleration = -10
var _vel_multiplier = 4

# Keyboard state
var _w = false
var _s = false
var _a = false
var _d = false
var _q = false
var _e = false

func _input(event):
	if use_mouse:
		# Receives mouse motion
		if event is InputEventMouseMotion:
			_mouse_position = event.relative

		# Receives mouse button input
		if event is InputEventMouseButton:
			match event.button_index:
				MOUSE_BUTTON_RIGHT: # Only allows rotation if right click down
					Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED if event.pressed else Input.MOUSE_MODE_VISIBLE)

	# Receives key input
	if event is InputEventKey:
		match event.scancode:
			KEY_W:
				_w = event.pressed
			KEY_S:
				_s = event.pressed
			KEY_A:
				_a = event.pressed
			KEY_D:
				_d = event.pressed
			KEY_Q:
				_q = event.pressed
			KEY_E:
				_e = event.pressed

# Updates mouselook and movement every frame
func _process(delta):
	_update_mouselook()
	_update_movement(delta)

# Updates camera movement
func _update_movement(delta):
	# Computes desired direction from key states
	_direction = Vector3(float(_d) - float(_a),
		float(_e) - float(_q),
		float(_s) - float(_w)) * _vel_multiplier

	var move_velocity_horizontal = Input.get_vector("gamepad_move_left", "gamepad_move_right", "gamepad_move_forward", "gamepad_move_backward") * _vel_multiplier
	var move_velocity_vertical = Input.get_axis("gamepad_move_down", "gamepad_move_up") * _vel_multiplier
	_direction += Vector3(move_velocity_horizontal[0], move_velocity_vertical, move_velocity_horizontal[1])

	# Computes the change in velocity due to desired direction and "drag"
	# The "drag" is a constant acceleration on the camera to bring it's velocity to 0
	var offset = _direction * _acceleration * _vel_multiplier * delta \
		+ _velocity * _deceleration * _vel_multiplier * delta
	
	# Checks if we should bother translating the camera
	if _direction == Vector3.ZERO and offset.length_squared() > _velocity.length_squared():
		# Sets the velocity to 0 to prevent jittering due to imperfect deceleration
		_velocity = Vector3.ZERO
	else:
		# Clamps speed to stay within maximum value (_vel_multiplier)
		_velocity.x = clamp(_velocity.x + offset.x, -_vel_multiplier, _vel_multiplier)
		_velocity.y = clamp(_velocity.y + offset.y, -_vel_multiplier, _vel_multiplier)
		_velocity.z = clamp(_velocity.z + offset.z, -_vel_multiplier, _vel_multiplier)
	
		translate(Vector3(1, 0, 1) * _velocity * delta)
		global_translate(Vector3.UP * _velocity * delta)

# Updates mouse look 
func _update_mouselook():
	var mouse_contribution = Vector2.ZERO
	# Only rotates mouse if the mouse is captured
	if use_mouse and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		mouse_contribution = _mouse_position * sensitivity
		_mouse_position = Vector2(0, 0)

	var gamepad_contribution = Vector2.ZERO
	var gamepad_look_velocity = Input.get_vector("gamepad_look_left", "gamepad_look_right", "gamepad_look_up", "gamepad_look_down")
	if gamepad_look_velocity.length_squared() > 0:
		gamepad_contribution = gamepad_look_velocity * sensitivity

	var total_velocity = mouse_contribution + gamepad_contribution
	var yaw = total_velocity.x
	var pitch = total_velocity.y	
	# Prevents looking up/down too far
	pitch = clamp(pitch, -90 - _total_pitch, 90 - _total_pitch)
	_total_pitch += pitch
	rotate_y(deg_to_rad(-yaw))
	rotate_object_local(Vector3(1,0,0), deg_to_rad(-pitch))
