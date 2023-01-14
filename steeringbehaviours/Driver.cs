using Godot;

namespace SteeringBehaviours {
    partial class Driver : CharacterBody3D {
        protected CharacterBody3DAgent Agent;
        protected SeekBehaviour SeekBehaviour;
        protected SteeringLocation Target = new SteeringLocation();
        protected TargetAcceleration Acceleration = new TargetAcceleration();
        [Export]
        public Vector3 TargetPosition;

        public override void _Ready() {
            Agent = new CharacterBody3DAgent(this);
            Agent.LinearSpeedMax = 1;
            Agent.LinearAccelerationMax = 10;
            Target.Position = TargetPosition;
            SeekBehaviour = new SeekBehaviour(Agent, Target);
        }

        public override void _PhysicsProcess(double delta)
        {
            SeekBehaviour.CalculateSteering(Acceleration);
            Agent.ApplySteering(Acceleration, (float) delta);
        }
    }
}