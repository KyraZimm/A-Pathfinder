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
    [Header("TEMP: Connected Players")]
    [SerializeField] PathFollower[] pathFollowers;

    Pathfinder pathfinder;

    public Grid<PathNode> MapGrid { get { return pathfinder.Grid; } }
    private Grid<GameObject> visualGrid;

    public bool EditMode { get; private set; }

    private void Awake() {
        EditMode = false;
        pathfinder = new Pathfinder(numMapCellsAcross, numMapCellsHigh, mapCellSize, mapOrigin);

        visualGrid = new Grid<GameObject>(numMapCellsAcross, numMapCellsHigh, mapCellSize, mapOrigin);

        for (int x = 0; x < numMapCellsAcross; x++) {
            for (int y = 0; y < numMapCellsHigh; y++) {
                GameObject cell = Instantiate(mapCellPrefab, pathfinder.Grid.GetCellWorldPos(x, y), Quaternion.identity);
                cell.transform.localScale = mapCellSize;

                visualGrid.SetValueAtCoords(x, y, cell);
            }
        }

        foreach (PathFollower follower in pathFollowers) {
            follower.Init(pathfinder);
        }
    }

    public List<PathNode> GetPath(Vector2 startWorldPos, Vector2 endWorldPos) {
        return pathfinder.GetPath(startWorldPos, endWorldPos);
    }

    public void ToggleEditMode() { EditMode = !EditMode; }

    private void Update() {
        if (!EditMode)
            return;

        if (Input.GetMouseButtonDown(0)) {
            ToggleNodeWalkability(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    private void ToggleNodeWalkability(Vector2 worldPos) {
        PathNode nodeToEdit = MapGrid.GetValueAtWorldPos(worldPos);
        nodeToEdit.isWalkable = !nodeToEdit.isWalkable;
        MapGrid.SetValueAtWorldPos(worldPos, nodeToEdit);

        visualGrid.GetValueAtWorldPos(worldPos).transform.GetChild(0).gameObject.SetActive(nodeToEdit.isWalkable);
    }
}
