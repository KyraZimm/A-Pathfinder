using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingMapMaker : MonoBehaviour {
    [Header("Map Size & Placement")]
    [SerializeField] int numMapCellsAcross;
    [SerializeField] int numMapCellsHigh;
    [SerializeField] Vector2 mapCellSize;
    [SerializeField] Vector2 mapOrigin;
    [Header("TEMP: Map Tile")]
    [SerializeField] GameObject mapCellPrefab;

    Pathfinder pathfinder;

    public Grid<PathNode> MapGrid { get { return pathfinder.Grid; } }

    private void Awake() {
        pathfinder = new Pathfinder(numMapCellsAcross, numMapCellsHigh, mapCellSize, mapOrigin);

        for (int x = 0; x < numMapCellsAcross; x++)
            for (int y = 0; y < numMapCellsHigh; y++)
                Instantiate(mapCellPrefab, pathfinder.Grid.GetCellWorldPos(x, y), Quaternion.identity);
    }

    public List<PathNode> GetPath(Vector2 startWorldPos, Vector2 endWorldPos) {
        return pathfinder.GetPath(startWorldPos, endWorldPos);
    }
}
