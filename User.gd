extends CSGSphere3D

# TODO: Replace with sets in C#
var near_trees: Dictionary = {}
var far_trees: Dictionary = {}

func _ready():
	var near_area = self.find_child("NearArea") as Area3D
	var far_area = self.find_child("FarArea") as Area3D
	near_area.body_entered.connect(near_area_entered)
	near_area.body_exited.connect(near_area_exited)
	far_area.body_entered.connect(far_area_entered)
	far_area.body_exited.connect(far_area_exited)

func near_area_entered(node: Node):
	near_trees[node.get_instance_id()] = node

func near_area_exited(node: Node):
	return near_trees.erase(node.get_instance_id())

func far_area_entered(node: Node):
	far_trees[node.get_instance_id()] = node

func far_area_exited(node: Node):
	return far_trees.erase(node.get_instance_id())

func _process(_delta):
	for key in far_trees:
		if key in near_trees:
			continue
		DebugDraw.draw_line_3d(self.global_transform.origin, far_trees.get(key).global_transform.origin, Color(0, 1, 0))
