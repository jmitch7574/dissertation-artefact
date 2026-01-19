## This is a simple script you can attach to a Camera3D node
## to turn it into a flying spectator camera.
## https://gist.github.com/TacticalLaptopBag/a9bdcea49f40dac5250e7c100e758f01

## Controls:
## Q: Camera Down
## E: Camera Up
## WASD: Move camera forward, left, down, right
## Mouse: Look
## Scroll wheel: Change camera speed
extends Camera3D

## The keycode to switch the debug camera on and off
@export var switch_keycode := KEY_QUOTELEFT
## The mouse sensitivity
@export var sensitivity := 1.0
## The camera default speed in units per second.
## This can change with the mousewheel.
@export var speed := 5.0

var _move_direction := Vector3.ZERO
var _enabled := false
var _previous_mouse_mode: int

func _ready():
	# Destroy the debug camera if this is a release build
	# This way your players can't accidentally activate the debug camera
	if not OS.is_debug_build():
		self.queue_free()
	
	self.current = false
	self.set_process(false)

func _unhandled_input(event: InputEvent):
	if event is InputEventKey:
		if event.is_pressed():
			# Key pressed
			
			# Toggle debug camera on/off
			if event.physical_keycode == switch_keycode:
				_enabled = not _enabled
				self.current = _enabled
				self.set_process(_enabled)
				
				if not _enabled:
					_move_direction = Vector3.ZERO
				else:
					_previous_mouse_mode = Input.mouse_mode
				
				Input.mouse_mode = Input.MOUSE_MODE_CAPTURED if _enabled else _previous_mouse_mode
			if not _enabled: return
			
			# Set _move_direction
			match event.physical_keycode:
				KEY_W:
					_move_direction.z = 1
				KEY_S:
					_move_direction.z = -1
				KEY_A:
					_move_direction.x = -1
				KEY_D:
					_move_direction.x = 1
				KEY_E:
					_move_direction.y = 1
				KEY_Q:
					_move_direction.y = -1
		else:
			# Key released
			
			# Set _move_direction
			match event.physical_keycode:
				KEY_W:
					if _move_direction.z < 0: return
					_move_direction.z = 0
				KEY_S:
					if _move_direction.z > 0: return
					_move_direction.z = 0
				KEY_A:
					if _move_direction.x > 0: return
					_move_direction.x = 0
				KEY_D:
					if _move_direction.x < 0: return
					_move_direction.x = 0
				KEY_E:
					if _move_direction.y < 0: return
					_move_direction.y = 0
				KEY_Q:
					if _move_direction.y > 0: return
					_move_direction.y = 0
	elif _enabled and event is InputEventMouseMotion:
		# Set camera rotation
		rotation_degrees += Vector3(-event.relative.y, -event.relative.x, 0) * sensitivity
	elif _enabled and event is InputEventMouseButton:
		# Change camera speed
		var delta := 0.0
		if event.button_index == MOUSE_BUTTON_WHEEL_UP:
			delta = 0.5
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			delta = -0.5
			
		if speed + delta <= 0: return
		speed += delta

func _process(delta: float):
	# Update camera position
	var normalized_move_direction = _move_direction.normalized()
	self.position += normalized_move_direction.x * transform.basis.x * delta * speed
	self.position += normalized_move_direction.y * transform.basis.y * delta * speed
	self.position += normalized_move_direction.z * -transform.basis.z * delta * speed
