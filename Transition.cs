using System.Collections.Generic;
using UnityEngine;

namespace Resolution {
    public struct MotionParameters {
        public float springConstant, dampingCoefficient, motionLimit;

        public MotionParameters(float springConstant, float dampingCoefficient, float motionLimit) {
            this.springConstant = springConstant;
            this.dampingCoefficient = dampingCoefficient;
            this.motionLimit = motionLimit;
        }
    };

    public class Transition {
        static public Dictionary<TransitionManager.Motion, MotionParameters> MotionParametersMap = new Dictionary<TransitionManager.Motion, MotionParameters> {
            { TransitionManager.Motion.Smooth, new MotionParameters(400.0f, 40.0f, 0.01f) },
            { TransitionManager.Motion.Bouncy, new MotionParameters(300.0f, 15.0f, 0.01f) }
        };

        public delegate void ScalarTransitionUpdateAction(float t);
        public delegate void VectorTransitionUpdateAction(Vector2 v);
        public delegate void TransitionDoneAction(Transition transition);
        public static event TransitionDoneAction OnTransitionDone;

        public Transition(TransitionManager.Motion motion, float delay, ParticleSystem particleSystem) {
            this.delay = delay;
            time = 0.0f;
            started = (delay == 0.0f);
            this.particleSystem = particleSystem;
            particle = particleSystem.AddParticle(1.0f, Vector2.zero, Vector2.zero);
            particleMassInv = 1.0f;
            motionParameters = MotionParametersMap[motion];
        }

        ~Transition() {
            particleSystem.RemoveParticle(particle);
        }

        public virtual void Update(float deltaTime) {
            if (!started) {
                time += deltaTime;

                if (time >= delay) {
                    started = true;
                }
            }
            else {
                Vector2 springForce = (v1 - particle.position) * motionParameters.springConstant;
                Vector2 dampingForce = -particle.velocity * motionParameters.dampingCoefficient;
                Vector2 force = springForce + dampingForce;
                Vector2 acceleration = force * particleMassInv;
                particle.velocity += acceleration * deltaTime;

                if (particle.velocity.sqrMagnitude + acceleration.sqrMagnitude < motionParameters.motionLimit) {
                    particle.velocity = Vector2.zero;
                    particle.position = v1;
                    OnTransitionDone?.Invoke(this);
                }
            }
        }

        protected Vector2 v0, v1;
        protected Particle particle;

        private float particleMassInv;
        private float delay;
        private float time;
        private bool started;
        private ParticleSystem particleSystem;
        private MotionParameters motionParameters;

    }

    public class ScalarTransition : Transition {
        public ScalarTransition(float t0, float t1, TransitionManager.Motion motion, float delay, ParticleSystem particleSystem, ScalarTransitionUpdateAction callback)
        : base(motion, delay, particleSystem) {
            v0 = new Vector2(t0, 0.0f);
            v1 = new Vector2(t1, 0.0f);
            this.callback = callback;
            particle.position = v0;
        }

        public override void Update(float deltaTime) {
            base.Update(deltaTime);
            callback.Invoke(particle.position.x);
        }

        private ScalarTransitionUpdateAction callback;
    }

    public class VectorTransition : Transition {
        public VectorTransition(Vector2 v0, Vector2 v1, TransitionManager.Motion motion, float delay, ParticleSystem particleSystem, VectorTransitionUpdateAction callback)
        : base(motion, delay, particleSystem) {
            this.v0 = v0;
            this.v1 = v1;
            this.callback = callback;
            particle.position = v0;
        }

        public override void Update(float deltaTime) {
            base.Update(deltaTime);
            callback.Invoke(particle.position);
        }

        private VectorTransitionUpdateAction callback;
    }
}
