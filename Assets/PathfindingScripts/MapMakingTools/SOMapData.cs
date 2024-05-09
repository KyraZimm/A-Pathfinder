using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {

    [CreateAssetMenu(fileName = "Map Data", menuName = "ScriptableObjects/Map Data")]
    public class SOMapData : ScriptableObject {
        public Grid<SavedPathNode> grid;

        public SOMapData(Grid<SavedPathNode> grid) {
            this.grid = grid;
        }
    }

}