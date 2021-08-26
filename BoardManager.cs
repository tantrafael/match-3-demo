using System.Collections.Generic;
using UnityEngine;

namespace Resolution {
    public struct Movement {
        public Vector2Int p0, p1;

        public Movement(Vector2Int p0, Vector2Int p1) {
            this.p0 = p0;
            this.p1 = p1;
        }
    }

    public class BoardManager : MonoBehaviour {
        public GameObject[] tiles;
        public int rows;
        public int columns;
        public int MatchLimit;
        public float TileSize;
        public float MoveDelayFactor;
        public float ScaleDelayFactor;

        private enum Action { PerformCollapse, PerformMatching };
        private GameGrid grid;
        private TransitionManager transitionManager;
        private Queue<Action> pendingActions;
        private List<Tile> removingTiles;
        private bool inputAllowed;

        void Awake() {
            transitionManager = new TransitionManager();
            pendingActions = new Queue<Action>();
            removingTiles = new List<Tile>();
            inputAllowed = true;
            Tile.OnHitTile += OnHitTile;
            TransitionManager.OnTransitionsDone += OnTranisitonsDone;
        }

        void FixedUpdate() {
            transitionManager.Update(Time.deltaTime);
        }

        void OnDestroy() {
            Tile.OnHitTile -= OnHitTile;
            TransitionManager.OnTransitionsDone -= OnTranisitonsDone;
        }

        public void SetupScene() {
            Vector2Int gridDimensions = new Vector2Int(columns, rows);
            grid = new GameGrid(gridDimensions);
            GenerateTiles();
        }

        private void GenerateTiles() {
            for (int row = 0; row < rows; ++row) {
                int TileTypesCount = tiles.Length;
                int accumulationType = -1;
                int accumulationCounter = 0;

                for (int col = 0; col < columns; ++col) {
                    int type = Mathf.RoundToInt((TileTypesCount - 1) * Random.value);
                    bool accumulationAtLimit = (accumulationCounter == MatchLimit - 1);

                    if (accumulationAtLimit) {
                        while (type == accumulationType) {
                            type = Mathf.RoundToInt((TileTypesCount - 1) * Random.value);
                        }
                    }

                    if (type != accumulationType) {
                        accumulationType = type;
                        accumulationCounter = 0;
                    }

                    accumulationCounter++;

                    Vector2Int gridPosition = new Vector2Int(col, row);
                    GameObject tileObject = Instantiate(tiles[type]);
                    Tile tile = tileObject.GetComponent<Tile>();
                    tile.Type = type;
                    tile.GridPosition = gridPosition;
                    tile.gameObject.transform.position = CalculateBoardPosition(gridPosition);
                    grid.SetTile(gridPosition, tile);
                }
            }
        }

        private Vector2 CalculateBoardPosition(Vector2Int gridPosition) {
            Vector2 boardPosition;
            boardPosition.x = gridPosition.x - 0.5f * columns + 0.5f * TileSize;
            boardPosition.y = -(gridPosition.y - 0.5f * rows + 0.5f * TileSize);

            return boardPosition;
        }

        private void OnHitTile(GameObject tileObject) {
            if (inputAllowed) {
                Tile tile = tileObject.GetComponent<Tile>();
                Scale(tile, 1.0f, 0.0f);
                removingTiles.Add(tile);
                pendingActions.Enqueue(Action.PerformCollapse);
                inputAllowed = false;
            }
        }

        private void OnTranisitonsDone() {
            if (pendingActions.Count > 0) {
                Action action = pendingActions.Dequeue();

                if (action == Action.PerformCollapse) {
                    PerformCollapse();
                }
                else if (action == Action.PerformMatching) {
                    PerformMatching();
                }
            }
        }

        private void PerformCollapse() {
            for (int i = 0, count = removingTiles.Count; i < count; ++i) {
                Tile tile = removingTiles[i];
                Vector2Int gridPosition = tile.GridPosition;
                Destroy(tile.gameObject);
                grid.ClearTile(gridPosition);
            }

            removingTiles.Clear();

            List<Movement> movements = CollapseGrid();

            if (movements.Count > 0) {
                MoveTiles(movements);
                pendingActions.Enqueue(Action.PerformMatching);
            }
            else {
                inputAllowed = true;
            }
        }

        public List<Movement> CollapseGrid() {
            List<Movement> movements = new List<Movement>();

            for (int col = 0; col < columns; ++col) {
                int lowestEmptyRow = rows;

                for (int row = rows - 1; row >= 0; --row) {
                    Vector2Int position = new Vector2Int(col, row);
                    Tile tile = grid.GetTile(position);

                    if (tile == null) {
                        if (lowestEmptyRow == rows) {
                            lowestEmptyRow = row;
                        }
                    }
                    else if (lowestEmptyRow < rows) {
                        Vector2Int lowestEmptyPosition = new Vector2Int(col, lowestEmptyRow);
                        movements.Add(new Movement(position, lowestEmptyPosition));
                        grid.ClearTile(position);
                        grid.SetTile(lowestEmptyPosition, tile);
                        --lowestEmptyRow;
                    }
                }
            }

            return movements;
        }

        private void PerformMatching() {
            List<Vector2Int> matchedPositions = FindMatches();

            if (matchedPositions.Count > 0) {
                for (int i = 0, count = matchedPositions.Count; i < count; ++i) {
                    Vector2Int gridPosition = matchedPositions[i];
                    Tile tile = grid.GetTile(gridPosition);
                    float delay = ScaleDelayFactor * i;
                    Scale(tile, 1.0f, 0.0f, delay);
                    removingTiles.Add(tile);
                }

                pendingActions.Enqueue(Action.PerformCollapse);
            }
            else {
                inputAllowed = true;
            }
        }

        private List<Vector2Int> FindMatches() {
            List<Vector2Int> matchedPositions = new List<Vector2Int>();
            List<Vector2Int> accumulator_row = new List<Vector2Int>();
            int accumulationType_row = -1;

            for (int row = 0; row < rows; ++row) {
                for (int col = 0; col < columns; ++col) {
                    Vector2Int gridPosition = new Vector2Int(col, row);
                    Tile tile = grid.GetTile(gridPosition);
                    int type = tile ? tile.Type : -1;
                    CheckForMatches(gridPosition, type, accumulator_row, ref accumulationType_row, matchedPositions, col, columns);
                    // This could be done here vertically as well, using the same function with additional column accumulators.
                }
            }

            return matchedPositions;
        }

        private void CheckForMatches(
            Vector2Int pos,
            int type,
            List<Vector2Int> accumulator,
            ref int accumulationType,
            List<Vector2Int> matchedPositions,
            int index,
            int limit
        )
        {
            // Encoutering a different type than the accumulated
            if (type != accumulationType) {
                if (accumulator.Count >= MatchLimit) {
                    matchedPositions.AddRange(accumulator);
                }

                accumulator.Clear();
                accumulationType = type;
            }

            // Accumulate non-empty positions
            if (type != -1) {
                accumulator.Add(pos);
            }

            // Reaching end of row or column
            if (index == limit - 1) {
                if (accumulator.Count >= MatchLimit)
                {
                    matchedPositions.AddRange(accumulator);
                }

                accumulator.Clear();
                accumulationType = -1;
            }
        }

        private void MoveTiles(List<Movement> movements) {
            for (int i = 0, count = movements.Count; i < count; ++i) {
                Movement movement = movements[i];
                Tile tile = grid.GetTile(movement.p1);
                Vector2 pos_0 = CalculateBoardPosition(movement.p0);
                Vector2 pos_1 = CalculateBoardPosition(movement.p1);
                tile.GridPosition = movement.p1;
                float delay = MoveDelayFactor * i;
                Move(tile, pos_0, pos_1, delay);
            }
        }

        private void Scale(Tile tile, float scale_0, float scale_1, float delay = 0.0f) {
            transitionManager.MakeTransition(scale_0, scale_1, TransitionManager.Motion.Smooth, delay, tile.SetScale);
        }

        private void Move(Tile tile, Vector2 pos_0, Vector2 pos_1, float delay) {
            transitionManager.MakeTransition(pos_0, pos_1, TransitionManager.Motion.Bouncy, delay, tile.SetPosition);
        }
    }
}
