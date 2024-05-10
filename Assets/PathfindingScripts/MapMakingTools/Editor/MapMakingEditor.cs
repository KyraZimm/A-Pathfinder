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
    public VisualTreeAsset testUXML;

    //file import & export
    private SOMapData file;
    const string EXPORT_FOLDER_PATH = "Assets/MapData";
    string newFileName;

    //layout
    private IMGUIContainer editMapPanel;
    private IMGUIContainer newMapPanel;

    //ui controls
    private Button newSaveFileButton;
    private Button editButton;
    private Toggle showGridToggle;

    //visualization
    bool editMode = false;
    bool editing = false;
    int lastEditedX = 0;
    int lastEditedY = 0;
    
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
        ToolbarButton showEditPanel = rootVisualElement.Q<ToolbarButton>("edit");
        ToolbarButton showNewMapPanel = rootVisualElement.Q<ToolbarButton>("newmap");
        showEditPanel.clicked += DisplayEditPanel;
        showNewMapPanel.clicked += DisplayNewMapPanel;

        //button funcs
        newSaveFileButton = newMapPanel.Q<Button>("createnewmap");
        newSaveFileButton.clicked += CreateNewMapFile;

        editButton = editMapPanel.Q<Button>("editmap");
        editButton.clicked += OnEditModeButton;

        //set up toggle
        showGridToggle = editMapPanel.Q<Toggle>("showgrid");
        showGridToggle.RegisterValueChangedCallback(OnShowMapToggle);

        //set up file name field
        TextField fileNameField = newMapPanel.Q<TextField>("filename");
        fileNameField.RegisterValueChangedCallback(OnNewFileNameChanged);
        newFileName = fileNameField.value;
        ValidateFileName(newFileName);

        //set up map file location to save to
        ObjectField saveFileField = rootVisualElement.Q<ObjectField>("savefile");
        saveFileField.RegisterValueChangedCallback(OnSaveFileChangedEvent);

        SOMapData currFile = (SOMapData)saveFileField.value;
        SetNewSaveFile(currFile);

        //load edit panel as initial view
        DisplayEditPanel();
    }

    private void DisplayEditPanel() {
        editMapPanel.style.display = DisplayStyle.Flex;
        newMapPanel.style.display = DisplayStyle.None;
        ControlEditPanelState();
    }

    private void DisplayNewMapPanel() {
        editMapPanel.style.display = DisplayStyle.None;
        newMapPanel.style.display = DisplayStyle.Flex;
        ControlEditPanelState();
    }

    private void ControlEditPanelState() {
        //if edit panel is not up, disable functionality
        if (editMapPanel.style.display != DisplayStyle.Flex) {
            DisableEditPanel();
            return;
        }

        bool saveFilePresent = file != null;
        bool gridIsShowing = showGridToggle.value;

        showGridToggle.SetEnabled(saveFilePresent);
        editButton.SetEnabled(gridIsShowing);
    }

    private void OnDisable() {
        DisableEditPanel();
    }

    private void DisableEditPanel() {
        //toggle off grid
        if (showGridToggle.enabledSelf)
            showGridToggle.value = false;
        SceneView.duringSceneGui -= ProjectGrid;

        //disable edit mode
        editMode = true;
        editing = false;
        SceneView.duringSceneGui -= EditMode;
    }
    #endregion


    #region File Handling
    private void OnSaveFileChangedEvent(ChangeEvent<UnityEngine.Object> evt) {
        SOMapData newFile = (SOMapData)evt.newValue;
        SetNewSaveFile(newFile);
    }

    private void SetNewSaveFile(SOMapData newFile) {
        file = newFile;
        ControlEditPanelState();
    }

    private void OnNewFileNameChanged(ChangeEvent<string> evt) {
        newFileName = evt.newValue;
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
        SOMapData newMapData = ScriptableObject.CreateInstance<SOMapData>();
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


    #region Visualization & Editing
    private void OnShowMapToggle(ChangeEvent<bool> evt) {
        if (evt.newValue) {
            //show grid
            SceneView.duringSceneGui += ProjectGrid;
            ProjectGrid(SceneView.currentDrawingSceneView);

            ControlEditPanelState();
        }
        else {
            //hide grid
            SceneView.duringSceneGui -= ProjectGrid;

            //disallow edit mode
            ControlEditPanelState();
            if (editMode)
                OnEditModeButton();
        }
    }

    private void OnEditModeButton() {
        editMode = !editMode;

        if (editMode)
            SceneView.duringSceneGui += EditMode;
        else
            SceneView.duringSceneGui -= EditMode;
    }

    private void ProjectGrid(SceneView sceneView) {
        Vector2 cellSize = file.grid.GetCellSize();
        Vector2 offset = new Vector2(-cellSize.x/2, -cellSize.y/2);
        for (int x = 0; x < file.grid.GetWidth(); x++) {
            for (int y = 0; y < file.grid.GetHeight(); y++) {
                Vector2 rectOrigin = file.grid.GetCellWorldPos(x, y) + offset; //get top-left corner of node
                Rect rect = new Rect(rectOrigin.x, rectOrigin.y, cellSize.x, cellSize.y);

                Color faceColor = file.grid.GetValueAtCoords(x, y).isWalkable ? Color.clear : Color.white;
                Handles.DrawSolidRectangleWithOutline(rect, faceColor, Color.white);
            }
        }
    }


    private void EditMode(SceneView sceneView) {
        Event e = Event.current;
        if (!e.isMouse)
            return;

        //check if edit is allowed
        if (e.type == EventType.MouseDown && e.button == 0)
            editing = true;
        else if (e.type == EventType.MouseUp && e.button == 0)
            editing = false;

        //control edit state
        if (editing) {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector2 pos = (Vector2)ray.origin;
            SavedPathNode node = file.grid.GetValueAtWorldPos(pos);

            if ((lastEditedX != node.x) || (lastEditedY != node.y)) {
                node.isWalkable = !node.isWalkable;
                lastEditedX = node.x;
                lastEditedY = node.y;
            }
        }
        else {
            lastEditedX = 0;
            lastEditedY = 0;
        }
    }
    #endregion
}
