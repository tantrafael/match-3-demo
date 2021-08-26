using UnityEngine;

namespace Resolution {
    public class Tile : MonoBehaviour {
        public delegate void HitAction(GameObject tile);
        public static event HitAction OnHitTile;

        public Vector2Int GridPosition { get; set; }
        public int Type { get; set; }

        private Vector3 initialScale;

        private void Awake() {
            initialScale = transform.localScale;
        }

        void OnMouseDown() {
            OnHitTile?.Invoke(gameObject);
        }

        public void SetScale(float scale) {
            transform.localScale = scale * initialScale;
        }

        public void SetPosition(Vector2 position) {
            transform.position = new Vector3(position.x, position.y, 0.0f);
        }
    }
}
