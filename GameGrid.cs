using System.Collections.Generic;
using UnityEngine;

namespace Resolution {
    public class GameGrid {
        private Vector2Int dimensions;
        private Tile[,] cells;

        public GameGrid(Vector2Int dimensions) {
            this.dimensions = dimensions;
            cells = new Tile[dimensions.y, dimensions.x];
        }

        public Tile GetTile(Vector2Int position) {
            Tile tile = null;

            if (IsValidPosition(position)) {
                tile = cells[position.y, position.x];
            }

            return tile;
        }

        public void SetTile(Vector2Int position, Tile tile) {
            if (IsValidPosition(position)) {
                cells[position.y, position.x] = tile;
            }
        }

        public void ClearTile(Vector2Int position) {
            SetTile(position, null);
        }

        private bool IsValidPosition(Vector2Int position) {
            return ((position.x >= 0) && (position.x < dimensions.x) && (position.y >= 0) && (position.y < dimensions.y));
        }
    }
}
