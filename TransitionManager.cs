using System.Collections.Generic;
using UnityEngine;

namespace Resolution {
    public class TransitionManager {
        public enum Motion { Smooth, Bouncy };

        public delegate void TransitionsDoneAction();
        public static event TransitionsDoneAction OnTransitionsDone;

        public TransitionManager() {
            transitions = new List<Transition>();
            particleSystem = new ParticleSystem();
            Transition.OnTransitionDone += OnTransitionDone;
        }

        ~TransitionManager() {
            Transition.OnTransitionDone -= OnTransitionDone;
        }

        public bool TransitionsInProgress() {
            return (transitions.Count > 0);
        }

        public void MakeTransition(float t0, float t1, TransitionManager.Motion motion, float delay, Transition.ScalarTransitionUpdateAction callback) {
            Transition transition = new ScalarTransition(t0, t1, motion, delay, particleSystem, callback);
            transitions.Add(transition);
        }

        public void MakeTransition(Vector2 v0, Vector2 v1, TransitionManager.Motion motion, float delay, Transition.VectorTransitionUpdateAction callback) {
            Transition transition = new VectorTransition(v0, v1, motion, delay, particleSystem, callback);
            transitions.Add(transition);
        }

        public void Update(float deltaTime) {
            // The number of transitions is recalculated each loop here,
            // since they are being removed as they are done.
            for (int i = 0; i < transitions.Count; ++i) {
                Transition transition = transitions[i];
                transition.Update(deltaTime);
            }

            particleSystem.Update(deltaTime);
        }

        private void OnTransitionDone(Transition transition) {
            transitions.Remove(transition);

            if (transitions.Count == 0) {
                OnTransitionsDone?.Invoke();
            }
        }

        private List<Transition> transitions;
        private ParticleSystem particleSystem;
    }
}
