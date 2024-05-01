using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingMapMaker : MonoBehaviour {
    [Header("Map Size & Placement")]
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] Vector2 cellSize;
    [SerializeField] Vector2 origin;
    [Header("TEMP: Map Tile")]
    [SerializeField] GameObject cellPrefab;

    Pathfinder pathfinder;
    PathFollower[] pathFollowers;

    public Grid<PathNode> MapGrid { get { return pathfinder.Grid; } }
    private Grid<GameObject> visualGrid;

    public bool EditMode { get; private set; }
    private bool editModeLastFrame;

    private void Awake() {
        EditMode = false;
        editModeLastFrame = EditMode;

        pathfinder = new Pathfinder(width, height, cellSize, origin);
        visualGrid = new Grid<GameObject>(width, height, cellSize, origin);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GameObject cell = Instantiate(cellPrefab, pathfinder.Grid.GetCellWorldPos(x, y), Quaternion.identity);
                cell.transform.localScale = cellSize;

                visualGrid.SetValueAtCoords(x, y, cell);
            }
        }

        pathFollowers = GameObject.FindObjectsOfType<PathFollower>();
        foreach (PathFollower follower in pathFollowers) {
            follower.Init(pathfinder);
        }
    }

    public List<PathNode> GetPath(Vector2 startWorldPos, Vector2 endWorldPos) {
        return pathfinder.GetPath(startWorldPos, endWorldPos);
    }

    public void ToggleEditMode() { EditMode = !EditMode; }

    private void Update() {
        if (EditMode != editModeLastFrame) {
            foreach (PathFollower follower in pathFollowers)
                follower.AllowMovement(!EditMode);

            editModeLastFrame = EditMode;
        }

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
