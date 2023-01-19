using Godot;
using System.Collections.Generic;

namespace WaypointPlanning {
    public partial class Driver : Node {
        public WaypointPlanner WaypointPlanner = new WaypointPlanner();
        public ImmediateMesh ImmediateMesh;

        public override void _Ready() {
            ImmediateMesh = new ImmediateMesh();
            MeshInstance3D meshInstance3D = new MeshInstance3D();
            meshInstance3D.Mesh = ImmediateMesh;
            AddChild(meshInstance3D);
            StandardMaterial3D material = new StandardMaterial3D();
            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.VertexColorUseAsAlbedo = true;
            meshInstance3D.MaterialOverride = material;

            List<Waypoint> waypoints = new List<Waypoint>();
            Node treesNode = GetTree().Root.GetNode("root/Trees");
            foreach (var child in treesNode.GetChildren()) {
                if (child is Node3D) {
                    Vector3 position3d = (child as Node3D).GlobalPosition;
                    waypoints.Add(
                        new Waypoint(
                            new Vector2(position3d.x, position3d.z)
                        )
                    );
                }
            }
            IEnumerable<Edge> edges = WaypointPlanner.CreateGraph(waypoints);
            DrawWaypointGraph(edges);
        }

        public void DrawWaypointGraph(IEnumerable<Edge> edges) {
            ImmediateMesh.ClearSurfaces();
            ImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
            foreach (var edge in edges) {
                ImmediateMesh.SurfaceSetColor(Colors.Red);
                ImmediateMesh.SurfaceAddVertex(new Vector3(
                    edge.WaypointA.Position.x,
                    1,
                    edge.WaypointA.Position.y
                ));
                ImmediateMesh.SurfaceAddVertex(new Vector3(
                    edge.WaypointB.Position.x,
                    1,
                    edge.WaypointB.Position.y
                ));
            }
            ImmediateMesh.SurfaceEnd();
        }
    }
}