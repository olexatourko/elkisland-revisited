using Godot;

namespace SteeringBehaviours {
    // Extend Resource to allow SteeringLocation to be exported in Godot editor
    // https://godotengine.org/qa/51552/export-custom-type-variable-on-inspector-mono-c%23
    public partial class SteeringLocation {
        [Export]
        public Vector3 Position;
        [Export]
        public float Orientation;

        public SteeringLocation() {}

        public SteeringLocation(Vector3 position, float orientation) {
            Position = position;
            Orientation = orientation;
        }
    }
}