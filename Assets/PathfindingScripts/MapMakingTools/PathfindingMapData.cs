using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Map Data", menuName = "ScriptableObjects/Map Data")]
public class PathfindingMapData : ScriptableObject {
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] Vector2 origin;
    [SerializeField] Vector2 cellSize;

    [SerializeField] Serializable2DArray<SavedPathNode> cells;

    public PathfindingMapData(Grid<SavedPathNode> grid) {
        width = grid.GetWidth();
        height = grid.GetHeight();
        origin = grid.GetOrigin();
        cellSize = grid.GetCellSize();
        cells = new Serializable2DArray<SavedPathNode>(grid.GetArray());
    }

    public void WriteData(Grid<SavedPathNode> grid) {
        width = grid.GetWidth();
        height = grid.GetHeight();
        origin = grid.GetOrigin();
        cellSize = grid.GetCellSize();
        cells = new Serializable2DArray<SavedPathNode>(grid.GetArray());
    }

    public Grid<SavedPathNode> ToGrid(){ return new Grid<SavedPathNode>(width, height, cellSize, origin, cells.Array); }
}
