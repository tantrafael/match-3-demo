using System.Collections.Generic;
using UnityEngine;

namespace Resolution {
    public class ParticleSystem {
        public ParticleSystem() {
            particles = new List<Particle>();
        }

        public Particle AddParticle(float mass, Vector2 position, Vector2 velocity) {
            Particle particle = new Particle(mass, position, velocity);
            particles.Add(particle);

            return particle;
        }

        public void RemoveParticle(Particle particle) {
            particles.Remove(particle);
        }

        public void Update(float deltaTime) {
            for (int i = 0, count = particles.Count; i < count; ++i) {
                Particle particle = particles[i];
                particle.position += particle.velocity * deltaTime;
            }
        }

        private List<Particle> particles;
    }
}
