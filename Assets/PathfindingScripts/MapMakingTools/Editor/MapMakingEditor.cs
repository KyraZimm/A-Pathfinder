using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

public class MapMakingEditor : EditorWindow {
    public VisualTreeAsset testUXML;

    //file import & export
    private PathfindingMapData file;
    const string EXPORT_FOLDER_PATH = "Assets/MapData";
    string newFileName;

    //layout
    private IMGUIContainer editMapPanel;
    private IMGUIContainer newMapPanel;

    //ui controls
    private Button saveButton;
    private Button newSaveFileButton;

    //visualization
    /*int width;
    int height;
    Vector2 cellSize;
    Vector2 origin;*/
    
    [MenuItem("Pathfinder Tools/Map Maker")] public static void ShowEditor() {
        EditorWindow wnd = GetWindow<MapMakingEditor>();
        wnd.titleContent = new GUIContent("Map Maker");
    }

    #region Layout Updates & Setup
    public void CreateGUI() {
        testUXML.CloneTree(rootVisualElement);

        //ref different panels
        editMapPanel = rootVisualElement.Q<IMGUIContainer>("editmappanel");
        newMapPanel = rootVisualElement.Q<IMGUIContainer>("newmappanel");

        //set up toolbar buttons for panel selection
        ToolbarButton editMap = rootVisualElement.Q<ToolbarButton>("edit");
        ToolbarButton newMap = rootVisualElement.Q<ToolbarButton>("newmap");
        editMap.clicked += DisplayEditPanel;
        newMap.clicked += DisplayNewMapPanel;

        //button funcs
        saveButton = rootVisualElement.Q<Button>("savemap");
        //saveButton.RegisterCallback<MouseUpEvent>((evt) => mapMaker.SaveCurrMapData());
        
        newSaveFileButton = newMapPanel.Q<Button>("createnewmap");
        newSaveFileButton.clicked += CreateNewMapFile;

        //set up file name field
        TextField fileNameField = newMapPanel.Q<TextField>("filename");
        fileNameField.RegisterValueChangedCallback(OnNewFileNameChanged);
        newFileName = fileNameField.value;
        ValidateFileName(newFileName);

        //set up map file location to save to
        ObjectField saveFileField = rootVisualElement.Q<ObjectField>("savefile");
        saveFileField.RegisterValueChangedCallback(OnSaveFileChangedEvent);

        PathfindingMapData currFile = (PathfindingMapData)saveFileField.value;
        SetNewSaveFile(currFile);

        //set up toggle
        Toggle showGridToggle = editMapPanel.Q<Toggle>("showgrid");
        showGridToggle.RegisterValueChangedCallback(OnShowMapToggle);

        //load edit panel as initial view
        DisplayEditPanel();
    }

    private void DisplayEditPanel() {
        editMapPanel.style.display = DisplayStyle.Flex;
        newMapPanel.style.display = DisplayStyle.None;
    }

    private void DisplayNewMapPanel() {
        editMapPanel.style.display = DisplayStyle.None;
        newMapPanel.style.display = DisplayStyle.Flex;
    }
    #endregion


    #region File Handling
    private void OnSaveFileChangedEvent(ChangeEvent<UnityEngine.Object> evt) {
        PathfindingMapData newFile = (PathfindingMapData)evt.newValue;
        SetNewSaveFile(newFile);
    }

    private void SetNewSaveFile(PathfindingMapData newFile) {
        file = newFile;
        saveButton.SetEnabled(file != null);
    }

    private void OnNewFileNameChanged(ChangeEvent<string> evt) {
        ValidateFileName(evt.newValue);
    }

    private void ValidateFileName(string candidateName) {
        string path = EXPORT_FOLDER_PATH + "/" + candidateName + ".asset";
        newSaveFileButton.SetEnabled(!File.Exists(path));
    }

    private void CreateNewMapFile() {
        
        //get values for new save file
        int width = newMapPanel.Q<IntegerField>("width").value;
        int height = newMapPanel.Q<IntegerField>("height").value;
        Vector2 cellSize = newMapPanel.Q<Vector2Field>("cellsize").value;
        Vector2 origin = newMapPanel.Q<Vector2Field>("origin").value;

        //generate new save file w/ blank grid
        PathfindingMapData newMapData = ScriptableObject.CreateInstance<PathfindingMapData>();
        Grid<SavedPathNode> newMap = new Grid<SavedPathNode>(width, height, cellSize, origin);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                SavedPathNode nodeToSave = new SavedPathNode(x, y, true);
                newMap.SetValueAtCoords(x, y, nodeToSave);
            }
        }
        newMapData.grid = newMap;

        //generate file at preferred path
        string savePath = EXPORT_FOLDER_PATH + "/" + newFileName + ".asset";
        AssetDatabase.CreateAsset(newMapData, savePath);

        Debug.Log($"New save file {newFileName} was created at {savePath}.");
        AssetDatabase.Refresh();

        //update selected map file
        ObjectField saveFileField = rootVisualElement.Q<ObjectField>("savefile");
        saveFileField.value = newMapData;

        //since file exists now, validate file name to diable button
        ValidateFileName(newFileName);
    }
    #endregion


    #region Visualization In Scene
    private void OnShowMapToggle(ChangeEvent<bool> evt) {
        if (evt.newValue) {
            SceneView.duringSceneGui += ProjectGrid;
            ProjectGrid(SceneView.currentDrawingSceneView);
        }
        else {
            SceneView.duringSceneGui -= ProjectGrid;
        }
    }
    private void ProjectGrid(SceneView sceneView) {
        if (file == null)
            return;

        float trueWidth = file.grid.GetWidth() * file.grid.GetCellSize().x;
        float trueHeight = file.grid.GetHeight() * file.grid.GetCellSize().y;
        Vector2 offset = file.grid.GetCellSize() / 2;

        Vector2 pos = Vector2.zero;
        for (int x = 0; x < file.grid.GetWidth(); x++) {
            pos = file.grid.GetCellWorldPos(x, 0) - offset;
            Handles.DrawLine(pos, pos+(Vector2.up*trueHeight));
        }
        pos.x += offset.x * 2;
        Handles.DrawLine(pos, pos + (Vector2.up * trueHeight));

        for (int y = 0; y < file.grid.GetHeight(); y++) {
            pos = file.grid.GetCellWorldPos(0, y) - offset;
            Handles.DrawLine(pos, pos + (Vector2.right * trueWidth));
        }
        pos.y += offset.y * 2;
        Handles.DrawLine(pos, pos + (Vector2.right * trueWidth));
    }
    #endregion
}
