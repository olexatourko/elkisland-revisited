using Godot;

namespace SteeringBehaviours {
    public class SeekBehaviour : SteeringBehaviour {
        public SteeringLocation Target;

        public SeekBehaviour(SteeringAgent Agent) : base(Agent) {}
        public SeekBehaviour(SteeringAgent Agent, SteeringLocation target) : base(Agent) {
            Target = target;
        }
        public override void CalculateSteering(TargetAcceleration acceleration) {
            if (Target != null) {
                acceleration.Linear = (Target.Position - Agent.Location.Position).Normalized() * Agent.LinearAccelerationMax;
                acceleration.Angular = 0;
            }
        }
    }
}