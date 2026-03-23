@tool
extends Node

@export_tool_button("Generate City!", "Callable")
var generate_world = _generate

func _generate():
	# Find the C# manager and call it
	get_parent().Generate()
