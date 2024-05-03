using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PathfindingMapMaker : MonoBehaviour {
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] Vector2 cellSize;
    [SerializeField] Vector2 origin;
    [SerializeField] string newSaveFileName;

    [SerializeField] PathfindingMapData data;
    [SerializeField] GameObject cellPrefab;

    Pathfinder pathfinder;
    PathFollower[] pathFollowers;

    private Grid<PathNode> mapGrid { get { return pathfinder.Grid; } }
    private Grid<GameObject> visualGrid;

    public bool EditMode { get; private set; }
    private bool editModeLastFrame;

    private const string EXPORT_FOLDER_PATH = "Assets/MapData";

    private void Awake() {
        EditMode = false;
        editModeLastFrame = EditMode;

        //make sure save data is available & filled
        if (data == null)
            NewMapData(true);
        else if (data.grid == null || (data.grid.GetWidth() < 1 && data.grid.GetHeight() < 1))
            data.grid = WriteNewSaveData();

        //instantiate map from saved data
        pathfinder = new Pathfinder(data.grid);

        //TEMP: this will be replaced once I know what kind of visuals the map editor needs to render
        //render map using temp cell prefabs
        visualGrid = new Grid<GameObject>(data.grid.GetWidth(), data.grid.GetHeight(), data.grid.GetCellSize(), data.grid.GetOrigin());
        for (int x = 0; x < mapGrid.GetWidth(); x++) {
            for (int y = 0; y < mapGrid.GetHeight(); y++) {
                GameObject cell = Instantiate(cellPrefab, pathfinder.Grid.GetCellWorldPos(x, y), Quaternion.identity);
                cell.transform.localScale = cellSize;
                cell.transform.GetChild(0).gameObject.SetActive(mapGrid.GetValueAtCoords(x,y).isWalkable);

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
        PathNode nodeToEdit = mapGrid.GetValueAtWorldPos(worldPos);
        nodeToEdit.isWalkable = !nodeToEdit.isWalkable;
        mapGrid.SetValueAtWorldPos(worldPos, nodeToEdit);

        //TEMP: this will be replaced with a more robust system once I know what the final look of the grid needs to be
        visualGrid.GetValueAtWorldPos(worldPos).transform.GetChild(0).gameObject.SetActive(nodeToEdit.isWalkable);
    }


    public void NewMapData(bool forceNewFile) {
        string savePath = EXPORT_FOLDER_PATH + "/" + newSaveFileName + ".asset";

        //check that save file doesn't already exist
        if (File.Exists(savePath)) {

            //TEMP: this is p expensive, trim this later once other tasks are up to date
            if (forceNewFile) { //if a new file needs to be made, add version numbers until a unique ID is found
                int ID = 0;
                while (File.Exists(savePath)) {
                    ID++;
                    savePath = EXPORT_FOLDER_PATH + "/" + newSaveFileName + ID.ToString() + ".asset";
                }
            }
            else { //else, do not make new file
                Debug.LogWarning($"A file named {newSaveFileName} already exists at {savePath}. Either choose a new file name, or use the [Save Map Data] option instead.");
                return;
            }
        }

        PathfindingMapData newSaveData = ScriptableObject.CreateInstance<PathfindingMapData>();
        newSaveData.grid = WriteNewSaveData();
        AssetDatabase.CreateAsset(newSaveData, savePath);

        Debug.Log($"New save file {newSaveFileName} was created at {savePath}.");
        AssetDatabase.Refresh();

        data = newSaveData;
    }

    public void SaveMapData() {
        if (data == null) {
            Debug.LogWarning("No save file in MapMaker. Please add one in the Inspector.");
            return;
        }

        data.grid = GetCurrSaveData();
        Debug.Log($"Map saved to {data.name}!");
    }

    Grid<SavedPathNode> GetCurrSaveData() {
        Grid<SavedPathNode> currMap = new Grid<SavedPathNode>(mapGrid.GetWidth(), mapGrid.GetHeight(), mapGrid.GetCellSize(), mapGrid.GetOrigin());

        for (int x = 0; x < mapGrid.GetWidth(); x++) {
            for (int y = 0; y < mapGrid.GetHeight(); y++) {
                SavedPathNode nodeToSave = mapGrid.GetValueAtCoords(x, y).ToSaveData();
                currMap.SetValueAtCoords(x, y, nodeToSave);
            }
        }

        return currMap;
    }

    Grid<SavedPathNode> WriteNewSaveData() {
        Grid<SavedPathNode> newMap = new Grid<SavedPathNode>(width, height, cellSize, origin);

        for (int x = 0; x < width ; x++) {
            for (int y = 0; y < height; y++) {
                SavedPathNode nodeToSave = new SavedPathNode(x, y, true);
                newMap.SetValueAtCoords(x, y, nodeToSave);
            }
        }

        return newMap;
    }


}
