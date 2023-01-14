using Godot;

namespace SteeringBehaviours {
    public class TargetAcceleration {
        public Vector3 Linear = Vector3.Zero;
        public float Angular = 0;

        public void SetZero() {
            Linear.x = 0;
            Linear.y = 0;
            Linear.y = 0;
            Angular = 0;
        }

        public void AddScaledAcceleration(TargetAcceleration acceleration, float scalar) {
            Linear += acceleration.Linear * scalar;
            Angular += acceleration.Angular * scalar;
        }

        public float GetMagnitudeSquared() {
            return Linear.LengthSquared() + (Angular * Angular);
        }

        public float GetMagnitude() {
            return Mathf.Sqrt(GetMagnitude());
        }
    }
}