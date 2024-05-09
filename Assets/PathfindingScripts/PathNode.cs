using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
    public class PathNode {
        public int x;
        public int y;
        public Vector2 worldPos;

        public int gCost;
        public int hCost;
        public int fCost { get { return gCost + hCost; } }

        public PathNode previousNode;
        public bool isWalkable = true;

        public PathNode(int x, int y, Grid<PathNode> parentGrid) {
            this.x = x;
            this.y = y;
            this.worldPos = parentGrid.GetCellWorldPos(x, y);
            isWalkable = true;
        }

        public PathNode(SavedPathNode savedNodeData, Grid<PathNode> parentGrid) {
            this.x = savedNodeData.x;
            this.y = savedNodeData.y;
            this.worldPos = parentGrid.GetCellWorldPos(x, y);
            isWalkable = savedNodeData.isWalkable;
        }

        public SavedPathNode ToSaveData() {
            return new SavedPathNode(x, y, isWalkable);
        }
    }

    [System.Serializable] public class SavedPathNode {
        public int x;
        public int y;
        public bool isWalkable;

        public SavedPathNode(int x, int y, bool isWalkable) {
            this.x = x;
            this.y = y;
            this.isWalkable = isWalkable;
        }
    }
}