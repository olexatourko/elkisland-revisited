namespace SteeringBehaviours {
    public abstract class SteeringBehaviour {
        public SteeringAgent Agent;

        public SteeringBehaviour(SteeringAgent Agent) {
            this.Agent = Agent;
        }

        public virtual void CalculateSteering(TargetAcceleration acceleration) {
            acceleration.SetZero();
        }
    }
}