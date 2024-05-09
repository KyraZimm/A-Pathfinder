using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {

    [CreateAssetMenu(fileName = "Map Data", menuName = "ScriptableObjects/Map Data")]
    public class PathfindingMapData : ScriptableObject {
        public Grid<SavedPathNode> grid;

        public PathfindingMapData(Grid<SavedPathNode> grid) {
            this.grid = grid;
        }
    }

}
