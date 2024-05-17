using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Pathfinding;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

public class MapMakingEditor : EditorWindow {
    public VisualTreeAsset uxml;

    //layout refs
    EnumField fileTypeField;
    Button saveButton;

    //file handling
    [SerializeField] private SaveUtils.SupportedFileTypes fileType;
    [SerializeField] private SOMapData SOfile;
    [SerializeField] private TextAsset JSONfile;
    const string EXPORT_FOLDER_PATH = "Assets/MapData";

    //map dimensions / visibility
    Grid<SavedPathNode> dispGrid;
    [SerializeField] int width = 1;
    [SerializeField] int height = 1;
    [SerializeField] Vector2 cellSize = Vector2.one;
    [SerializeField] Vector2 origin = Vector2.zero;


    #region Layout Updates & Setup
    [MenuItem("Pathfinder Tools/Map Maker")] public static void ShowEditor() {
        EditorWindow wnd = GetWindow<MapMakingEditor>();
        wnd.titleContent = new GUIContent("Map Maker");
    }

    private void CreateGUI() {
        uxml.CloneTree(rootVisualElement);

        //header setup
        fileTypeField = rootVisualElement.Q<EnumField>("filetype");
        fileTypeField.RegisterValueChangedCallback(OnHeaderChanged);

        ObjectField soFileField = rootVisualElement.Q<ObjectField>("sofile");
        soFileField.RegisterValueChangedCallback(OnFileChanged);

        ObjectField jsonFileField = rootVisualElement.Q<ObjectField>("jsonfile");
        jsonFileField.RegisterValueChangedCallback(OnFileChanged);

        UpdateHeader();

        //save func
        saveButton = rootVisualElement.Q<Button>("save");
        saveButton.clicked += SaveMapContents;

        Toggle showGridToggle = rootVisualElement.Q<Toggle>("showgrid");
        showGridToggle.RegisterValueChangedCallback(OnShowMapToggle);

        //add binding fields from UI Builder
        SerializedObject so = new SerializedObject(this);
        rootVisualElement.Bind(so);

        //inst. disp grid
        dispGrid = new Grid<SavedPathNode>(width, height, cellSize, origin);
    }

    //HEADER LAYOUT
    private void OnHeaderChanged(ChangeEvent<System.Enum> evt) { UpdateHeader((SaveUtils.SupportedFileTypes)evt.newValue); }
    private void UpdateHeader() {
        System.Enum val = fileTypeField.value;
        UpdateHeader((SaveUtils.SupportedFileTypes)val);
    }
    private void UpdateHeader(SaveUtils.SupportedFileTypes overrideValue) {
        fileType = overrideValue;
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                rootVisualElement.Q<ObjectField>("sofile").style.display = DisplayStyle.Flex;
                rootVisualElement.Q<ObjectField>("jsonfile").style.display = DisplayStyle.None;
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                rootVisualElement.Q<ObjectField>("sofile").style.display = DisplayStyle.None;
                rootVisualElement.Q<ObjectField>("jsonfile").style.display = DisplayStyle.Flex;
                break;
        }
    }

    //LAYOUT UPDATES
    private void OnInspectorUpdate() {
        UpdateWindowState();
    }

    private void UpdateWindowState() {
        bool saveFilePresent = (fileType == SaveUtils.SupportedFileTypes.SO && SOfile != null) || (fileType == SaveUtils.SupportedFileTypes.JSON && JSONfile != null);
        bool changesMade = EditorUtility.IsDirty(this);

        saveButton.SetEnabled(saveFilePresent && changesMade);
        UpdateDispGridSize();
    }

    private void OnDisable() { OnCloseWindow(); }
    private void OnDestroy() { OnCloseWindow(); }

    private void OnCloseWindow() {
        SceneView.duringSceneGui -= ProjectGrid;
    }

    #endregion


    #region File Handling
    private void OnFileChanged(ChangeEvent<UnityEngine.Object> evt) {
        //try to get grid values from file
        Grid<SavedPathNode> tryReadGrid = null;
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                if (SOfile == null) return;
                tryReadGrid = SOfile.grid;
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                if (JSONfile == null) return;
                tryReadGrid = JsonUtility.FromJson<Grid<SavedPathNode>>(JSONfile.text);
                break;
        }

        //if grid is present, change curr values to the read file
        if (tryReadGrid != null) {
            width = tryReadGrid.GetWidth();
            height = tryReadGrid.GetHeight();
            cellSize = tryReadGrid.GetCellSize();
            origin = tryReadGrid.GetOrigin();

            dispGrid = tryReadGrid;
        }

        //remove dirty flags
        EditorUtility.ClearDirty(this);
    }

    private void SaveMapContents() {
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                SOfile.grid = dispGrid;
                EditorUtility.SetDirty(SOfile);
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                SaveUtils.SaveToJson(JSONfile.name, dispGrid);
                break;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.ClearDirty(this);
    }
    #endregion


    #region Visualization & Editing
    private void UpdateDispGridSize() {
        //see if 2D grid array needs to be resized
        int widthDiff = width - dispGrid.GetWidth();
        int heightDiff = height - dispGrid.GetHeight();
        if (widthDiff != 0 || heightDiff != 0) {
            Grid<SavedPathNode> resized = new Grid<SavedPathNode>(width, height, cellSize, origin);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    SavedPathNode copyValue = dispGrid.GetValueAtCoords(x, y);
                    resized.SetValueAtCoords(x, y, copyValue);
                }
            }
            dispGrid = resized;
        }

        //make sure cell size and origin match
        if (dispGrid.GetCellSize() != cellSize) dispGrid.SetOrigin(cellSize);
        if (dispGrid.GetOrigin() != origin) dispGrid.SetOrigin(origin);
    }

    private void OnShowMapToggle(ChangeEvent<bool> evt) {
        if (evt.newValue) {
            //show grid
            SceneView.duringSceneGui += ProjectGrid;
            ProjectGrid(SceneView.currentDrawingSceneView);
        }
        else {
            //hide grid
            SceneView.duringSceneGui -= ProjectGrid;
        }
    }

    private void ProjectGrid(SceneView sceneView) {
        //if dispgrid needs to be updated, wait
        if (width != dispGrid.GetWidth() || height != dispGrid.GetHeight())
            return;

        Vector2 offset = new Vector2(-cellSize.x / 2, -cellSize.y / 2);
        for (int x = 0; x < dispGrid.GetWidth(); x++) {
            for (int y = 0; y < dispGrid.GetHeight(); y++) {
                Vector2 rectOrigin = dispGrid.GetCellWorldPos(x, y) + offset; //get top-left corner of node
                Rect rect = new Rect(rectOrigin.x, rectOrigin.y, cellSize.x, cellSize.y);

                SavedPathNode node = dispGrid.GetValueAtCoords(x, y);
                if (node == null) continue;
                Handles.DrawSolidRectangleWithOutline(rect, (node.isWalkable ? Color.clear : Color.white), Color.white);
            }
        }
    }
    #endregion
}
