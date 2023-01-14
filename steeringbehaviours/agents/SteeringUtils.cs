using Godot;

namespace SteeringBehaviours {
    public static class SteeringUtils {
        public static Vector3 Clamped(Vector3 vector, double limit) {
            double lengthSquared = vector.LengthSquared();
            double limitSquared = limit * limit;
            if (lengthSquared > limitSquared) {
                vector *= Mathf.Sqrt((float) limit / (float) lengthSquared);
            }
            return vector;
        }
    }
}