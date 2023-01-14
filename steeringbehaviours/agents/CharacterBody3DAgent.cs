// https://github.com/GDQuest/godot-steering-ai-framework/blob/master/godot/addons/com.gdquest.godot-steering-ai-framework/Agents/GSAIKinematicBody3DAgent.gd

using Godot;

namespace SteeringBehaviours {
    public enum MovementType {
        Slide,
        Collide,
        Position
    }

    public class CharacterBody3DAgent : SteeringAgent {

        private CharacterBody3D _body;
        public CharacterBody3D Body {
            get => _body;
            set {
                _body = value;
                LastPosition = value.Transform.origin;
                LastOrientation = value.Rotation.y;
            }
        }
        public MovementType MovementType;

        protected Vector3 LastPosition;
        protected float LastOrientation;

        public CharacterBody3DAgent(CharacterBody3D body) {
            Body = body;
            MovementType = MovementType.Slide;
            // Connect physics frame signals to method for updating position and velocities
            if (!Body.IsInsideTree()) {
                Body.Ready += () => {
                    // https://docs.godotengine.org/en/latest/classes/class_object.html#class-object-method-connect
                    Body.GetTree().PhysicsFrame += OnSceneTreePhysicsFrame;
                };
            } else {
                Body.GetTree().PhysicsFrame += OnSceneTreePhysicsFrame;
            }
        }

        public void ApplySteering(TargetAcceleration acceleration, float delta) {
            if (MovementType == MovementType.Collide) {
                ApplyCollideSteering(acceleration.Linear, delta);
            } else if (MovementType == MovementType.Slide) {
                ApplySlidingSteering(acceleration.Linear, delta);
            } else {
                ApplyPositionSteering(acceleration.Linear, delta);
            }
            ApplyOrientationSteering(acceleration.Angular, delta);
        }

        public void ApplySlidingSteering(Vector3 acceleration, float delta) {
            Vector3 velocity = SteeringUtils.Clamped(acceleration + (acceleration * delta), LinearSpeedMax);
            // TODO: Apply linear drag if applicable
            Body.Velocity = velocity;
            Body.MoveAndSlide();
            if (CalculateVelocities) LinearVelocity = velocity;
        }

        public void ApplyCollideSteering(Vector3 acceleration, float delta) {
            Vector3 velocity = SteeringUtils.Clamped(acceleration + (acceleration * delta), LinearSpeedMax);
            // TODO: Apply linear drag if applicable
            Body.MoveAndCollide(velocity * delta);
            if (CalculateVelocities) LinearVelocity = velocity;
        }

        public void ApplyPositionSteering(Vector3 acceleration, float delta) {
            Vector3 velocity = SteeringUtils.Clamped(acceleration + (acceleration * delta), LinearSpeedMax);
            // TODO: Apply linear drag if applicable
            Body.GlobalPosition += velocity * delta;
            if (CalculateVelocities) LinearVelocity = velocity;
        }

        public void ApplyOrientationSteering(float AngularAcceleration, float delta) {
            float velocity = Mathf.Clamp(AngularVelocity + (AngularAcceleration * delta),
                -AngularSpeedMax,
                AngularAccelerationMax
            );
            // TODO: Apply angular drag if applicable
            Body.RotateY(velocity * delta);
            if (CalculateVelocities) AngularVelocity = velocity;
        }

        public void OnSceneTreePhysicsFrame() {
            if (!CalculateVelocities) return;
            Location.Position = Body.Transform.origin;
            Location.Orientation = Body.Rotation.y;
            Vector3 currentPosition = Body.Transform.origin;
            float currentOrientation = Body.Rotation.y;

            if (AppliedSteering) {
                AppliedSteering = false;
            }
            else {
                LinearVelocity = SteeringUtils.Clamped(currentPosition - LastPosition, LinearSpeedMax);
                // TODO: Apply linear drag if applicable
                float velocity = Mathf.Clamp(LastOrientation - currentOrientation,
                    -AngularSpeedMax,
                    AngularAccelerationMax
                );
                // TODO: Apply angular drag if applicable

                LastPosition = currentPosition;
                LastOrientation = currentOrientation;
            }
        }
    }
}