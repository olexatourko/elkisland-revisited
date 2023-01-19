using Godot;
using System;
using System.Collections.Generic;
using WaypointPlanning;
using SteeringBehaviours;

namespace Shadow {
    public partial class UserControlledShadow : Node {
        protected WaypointPlanner WaypointPlanner = new WaypointPlanner();
        protected CharacterBody3DAgent Agent;
        protected SeekBehaviour SeekBehaviour;
        protected TargetAcceleration Acceleration = new TargetAcceleration();
        protected Dictionary<Waypoint, HashSet<Waypoint>> AdjacencyList;
        private Waypoint _currentTarget;
        private List<Waypoint> _candidateTargets;
        private Waypoint _currentCandidateTarget;

        // TODO collisions are in effect this large value temporarily accounts for them. Change this.
        protected const float TargetRangeThreshold = 1;
        protected const float CommonYPosition = 1;
        public Waypoint CurrentTarget {
            set {
                if (this.AdjacencyList == null || !this.AdjacencyList.ContainsKey(value)) {
                    throw new ArgumentException("Value doesn't exist in the adjacency list");
                }
                _currentTarget = value;
                SeekBehaviour.Target = new SteeringLocation(
                    new Vector3(value.Position.x, CommonYPosition, value.Position.y),
                    0
                );
                // Update canadidate target list
                _candidateTargets = new List<Waypoint>(AdjacencyList[value]);
                _currentCandidateTarget = _candidateTargets[0];
            }
            get {
                return _currentTarget;
            }
        }

        public bool IsAtCurrentTarget {
            get {
                if (CurrentTarget == null) return false;
                Vector2 agent2DPosition = new Vector2(Agent.Body.Position.x, Agent.Body.Position.z);
                return (CurrentTarget.Position - agent2DPosition).Length() < TargetRangeThreshold;
            }
        }
        public ImmediateMesh ImmediateMesh;

        public override void _Ready()
        {
            /// Setup the steering agent
            Agent = new CharacterBody3DAgent(FindChild("CharacterBody3D") as CharacterBody3D);
            Agent.MovementType = MovementType.Slide;
            Agent.LinearSpeedMax = 20;
            Agent.LinearAccelerationMax = Agent.LinearSpeedMax * 10;
            SeekBehaviour = new SeekBehaviour(Agent);

            // Get the set of waypoints to use
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
            AdjacencyList = new Dictionary<Waypoint, HashSet<Waypoint>>();
            foreach (var edge in edges) {
                if (!AdjacencyList.ContainsKey(edge.WaypointA)) AdjacencyList[edge.WaypointA] = new HashSet<Waypoint>(){edge.WaypointB};
                else AdjacencyList[edge.WaypointA].Add(edge.WaypointB);
                if (!AdjacencyList.ContainsKey(edge.WaypointB)) AdjacencyList[edge.WaypointB] = new HashSet<Waypoint>(){edge.WaypointA};
                else AdjacencyList[edge.WaypointB].Add(edge.WaypointA);
            }
            CurrentTarget = waypoints[0];

            // Setup graph renderer
            ImmediateMesh = new ImmediateMesh();
            MeshInstance3D meshInstance3D = new MeshInstance3D();
            meshInstance3D.Mesh = ImmediateMesh;
            AddChild(meshInstance3D);
            StandardMaterial3D material = new StandardMaterial3D();
            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.VertexColorUseAsAlbedo = true;
            meshInstance3D.MaterialOverride = material;

            DrawWaypointGraph(edges);
        }

        public override void _Input(InputEvent @event) {
            if (!IsAtCurrentTarget || _currentCandidateTarget == null) return;
            int currentCandidateTargetIdx = _candidateTargets.IndexOf(_currentCandidateTarget);
            if (@event.IsActionPressed("gamepad_left_bumper")) {
                currentCandidateTargetIdx -= 1;
                if (currentCandidateTargetIdx < 0) currentCandidateTargetIdx = _candidateTargets.Count - 1;
                _currentCandidateTarget = _candidateTargets[currentCandidateTargetIdx];
            } else if (@event.IsActionPressed("gamepad_right_bumper")) {
                currentCandidateTargetIdx += 1;
                if (currentCandidateTargetIdx == _candidateTargets.Count) currentCandidateTargetIdx = 0;
                _currentCandidateTarget = _candidateTargets[currentCandidateTargetIdx];
            } else if (@event.IsActionPressed("gamepad_a")) {
                CurrentTarget = _currentCandidateTarget;
            }
        }

        public override void _PhysicsProcess(double delta) {
            if (!IsAtCurrentTarget) {
                SeekBehaviour.CalculateSteering(Acceleration);
                Agent.ApplySteering(Acceleration, (float) delta);
            } else {
                DrawAdjacentEdges();
            }
        }

        public void DrawWaypointGraph(IEnumerable<Edge> edges) {
            ImmediateMesh.ClearSurfaces();
            ImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
            foreach (var edge in edges) {
                ImmediateMesh.SurfaceSetColor(Colors.Red);
                ImmediateMesh.SurfaceAddVertex(new Vector3(
                    edge.WaypointA.Position.x,
                    CommonYPosition,
                    edge.WaypointA.Position.y
                ));
                ImmediateMesh.SurfaceAddVertex(new Vector3(
                    edge.WaypointB.Position.x,
                    CommonYPosition,
                    edge.WaypointB.Position.y
                ));
            }
            ImmediateMesh.SurfaceEnd();
        }

        public void DrawAdjacentEdges() {
            ImmediateMesh.ClearSurfaces();
            ImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
            foreach (var adjacentWaypoint in AdjacencyList[CurrentTarget]) {
                if (adjacentWaypoint == _currentCandidateTarget) ImmediateMesh.SurfaceSetColor(Colors.Yellow);
                else ImmediateMesh.SurfaceSetColor(Colors.Green);
                ImmediateMesh.SurfaceAddVertex(new Vector3(
                    CurrentTarget.Position.x,
                    CommonYPosition,
                    CurrentTarget.Position.y
                ));
                ImmediateMesh.SurfaceAddVertex(new Vector3(
                    adjacentWaypoint.Position.x,
                    CommonYPosition,
                    adjacentWaypoint.Position.y
                )); 
            }
            ImmediateMesh.SurfaceEnd();
        }

        // When arrived at current target allow user to choose a new target directly connected by an edge        
    }   
}