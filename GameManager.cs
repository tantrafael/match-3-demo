using UnityEngine;

namespace Resolution {
    public class GameManager : MonoBehaviour {
        public static GameManager instance;
        private BoardManager boardManager;

        public GameManager() {
            instance = null;
        }

        void Awake() {
            if (instance == null) {
                instance = this;
            }
            else if (instance != this) {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            boardManager = GetComponent<BoardManager>();
            InitializeGame();
        }

        void OnDestroy() {
            DecomissionGame();
        }

        private void InitializeGame() {
            boardManager.SetupScene();
        }

        private void DecomissionGame() {
        }
    }
}
