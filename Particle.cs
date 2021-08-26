using UnityEngine;

namespace Resolution {
    public class Particle {
        public float mass;
        public Vector2 position;
        public Vector2 velocity;

        public Particle(float mass, Vector2 position, Vector2 velocity) {
            this.mass = mass;
            this.position = position;
            this.velocity = velocity;
        }
    }
}
