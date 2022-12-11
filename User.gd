extends CSGSphere

# TODO: Replace with sets in C#
var near_trees: Dictionary = {}
var far_trees: Dictionary = {}

func _ready():
	var near_area = find_node("NearArea") as Area
	var far_area = find_node("FarArea") as Area
	near_area.connect("body_entered", self, "near_area_entered")
	near_area.connect("body_exited", self, "near_area_exited")
	far_area.connect("body_entered", self, "far_area_entered")
	far_area.connect("body_exited", self, "far_area_exited")

func near_area_entered(node: Node):
	near_trees[node.get_instance_id()] = node

func near_area_exited(node: Node):
	return near_trees.erase(node.get_instance_id())

func far_area_entered(node: Node):
	far_trees[node.get_instance_id()] = node

func far_area_exited(node: Node):
	return far_trees.erase(node.get_instance_id())

func _process(delta):
	for key in far_trees:
		if key in near_trees:
			continue
		DebugDraw.draw_line_3d(self.global_transform.origin, far_trees.get(key).global_transform.origin, Color(0, 1, 0))

# Called every frame. 'delta' is the elapsed time since the previous frame.
# func _process(delta):
# 	pass
