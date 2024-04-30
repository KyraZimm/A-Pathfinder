using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map Data", menuName = "ScriptableObjects/Map Data")]
public class PathfindingMapData : ScriptableObject {
    public int height;
    public int width;
    public Vector2 cellSize;
    public Vector2 origin;

    public Serializable2DArray<bool> walkableNodes;

    /*public void GetWalkableNodes() {
        walkableNodes[]
    }*/
}
