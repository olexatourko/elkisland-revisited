using Godot;

namespace SteeringBehaviours {
    // Port of  https://github.com/GDQuest/godot-steering-ai-framework/blob/master/godot/addons/com.gdquest.godot-steering-ai-framework/Agents/GSAIKinematicBody3DAgent.gd
    public abstract partial class SteeringAgent {
        public SteeringLocation Location = new SteeringLocation();
        // The amount of velocity to be considered effectively not moving.
        public float ZeroLinearSpeedThreshold = 0.01f;
        // The maximum speed at which the agent can move.
        public float LinearSpeedMax = 0;
        // The maximum amount of acceleration that any behavior can apply to the agent.
        public float LinearAccelerationMax = 0;
        // The maximum amount of angular speed at which the agent can rotate.
        public float AngularSpeedMax = 0;
        // The maximum amount of angular acceleration that any behavior can apply to an agent.
        public float AngularAccelerationMax = 0;
        // Current velocity of the agent.
        public Vector3 LinearVelocity = Vector3.Zero;
        // Current angular velocity of the agent.
        public float AngularVelocity = 0;
        // The radius of the sphere that approximates the agent's size in space.
        public float BoundingRadius = 0;
    }
    // Port of https://github.com/GDQuest/godot-steering-ai-framework/blob/master/godot/addons/com.gdquest.godot-steering-ai-framework/Agents/GSAISpecializedAgent.gd
    public abstract partial class SteeringAgent {
        public bool CalculateVelocities = true;
        protected bool AppliedSteering = false;
    }
}