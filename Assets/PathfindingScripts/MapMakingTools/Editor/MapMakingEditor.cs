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
    private SaveUtils.SupportedFileTypes fileType;
    private SOMapData fileSO;
    private TextAsset fileJSON;
    const string EXPORT_FOLDER_PATH = "Assets/MapData";
    string newFileName;

    //layout
    private IMGUIContainer headerPanel;
    private IMGUIContainer editMapPanel;
    private IMGUIContainer newMapPanel;

    //ui controls
    private Button newSaveFileButton;
    private Button saveButton;
    private Button editButton;
    private Toggle showGridToggle;

    //visualization
    bool editMode = false;
    bool editing = false;
    int lastEditedX = 0;
    int lastEditedY = 0;
    Grid<SavedPathNode> dispGrid;
    
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
        headerPanel = rootVisualElement.Q<IMGUIContainer>("header");

        //set up toolbar buttons for panel selection
        ToolbarButton showEditPanel = rootVisualElement.Q<ToolbarButton>("edit");
        ToolbarButton showNewMapPanel = rootVisualElement.Q<ToolbarButton>("newmap");
        showEditPanel.clicked += DisplayEditPanel;
        showNewMapPanel.clicked += DisplayNewMapPanel;

        //button funcs
        newSaveFileButton = newMapPanel.Q<Button>("createnewmap");
        newSaveFileButton.clicked += CreateNewMapFile;

        saveButton = editMapPanel.Q<Button>("save");
        saveButton.clicked += SaveMapContents;

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

        //header setup
        headerPanel.Q<EnumField>("filetype").RegisterValueChangedCallback(OnHeaderChanged);

        ObjectField soFileField = rootVisualElement.Q<ObjectField>("sofile");
        soFileField.RegisterValueChangedCallback(OnSaveFileChangedEvent);
        fileSO = (SOMapData)soFileField.value;

        ObjectField jsonFileField = rootVisualElement.Q<ObjectField>("jsonfile");
        jsonFileField.RegisterValueChangedCallback(OnSaveFileChangedEvent);
        fileJSON = (TextAsset)jsonFileField.value;

        UpdateHeader();
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                SetNewSOFile(fileSO);
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                SetNewJSONFile(fileJSON);
                break;
        }

        //load edit panel as initial view
        DisplayEditPanel();

    }

    //PANEL SWITCHING
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

    //HEADER
    private void OnHeaderChanged(ChangeEvent<System.Enum> evt) { UpdateHeader((SaveUtils.SupportedFileTypes)evt.newValue); }
    private void UpdateHeader() {
        System.Enum val = headerPanel.Q<EnumField>("filetype").value;
        UpdateHeader((SaveUtils.SupportedFileTypes)val);
    }
    private void UpdateHeader(SaveUtils.SupportedFileTypes overrideValue) {
        fileType = overrideValue;
        switch (fileType){
            case SaveUtils.SupportedFileTypes.SO:
                headerPanel.Q<ObjectField>("sofile").style.display = DisplayStyle.Flex;
                headerPanel.Q<ObjectField>("jsonfile").style.display = DisplayStyle.None;
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                headerPanel.Q<ObjectField>("sofile").style.display = DisplayStyle.None;
                headerPanel.Q<ObjectField>("jsonfile").style.display = DisplayStyle.Flex;
                break;
        }

        PreloadGrid();
    }


    //EDIT PANEL
    private void ControlEditPanelState() {
        //if edit panel is not up, disable functionality
        if (editMapPanel.style.display != DisplayStyle.Flex) {
            DisableEditPanel();
            return;
        }

        bool saveFilePresent = (fileType == SaveUtils.SupportedFileTypes.SO && fileSO != null) || (fileType == SaveUtils.SupportedFileTypes.JSON && fileJSON != null);
        bool gridIsShowing = showGridToggle.value;
        //bool gridIsEdited = saveFilePresent && EditorUtility.IsDirty(dispGrid.id);

        showGridToggle.SetEnabled(saveFilePresent);
        editButton.SetEnabled(gridIsShowing);
        saveButton.SetEnabled(saveFilePresent && gridIsShowing);
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

    private void OnDisable() {
        DisableEditPanel();
    }
    #endregion


    #region File Handling
    private void OnSaveFileChangedEvent(ChangeEvent<UnityEngine.Object> evt) {
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                SetNewSOFile((SOMapData)evt.newValue);
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                SetNewJSONFile((TextAsset)evt.newValue);
                break;
        }
    }

    private void SetNewSOFile(SOMapData newFile) {
        fileSO = newFile;
        PreloadGrid();
        ControlEditPanelState();
    }

    private void SetNewJSONFile(TextAsset newFile) {
        fileJSON = newFile;
        PreloadGrid();
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

    private void SaveMapContents() {
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                fileSO.grid = dispGrid;
                EditorUtility.SetDirty(fileSO);
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                SaveUtils.SaveToJson(fileJSON.name, dispGrid);
                break;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ControlEditPanelState();
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

    private void PreloadGrid() {
        Grid<SavedPathNode> tryReadGrid = new Grid<SavedPathNode>(0, 0, Vector2.zero, Vector2.zero);
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                if (fileSO == null) return;
                tryReadGrid = fileSO.grid;
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                if (fileJSON == null) return;
                tryReadGrid = JsonUtility.FromJson<Grid<SavedPathNode>>(fileJSON.text);
                break;
        }

        if (tryReadGrid.GetWidth() > 0 && tryReadGrid.GetHeight() > 0)
            dispGrid = tryReadGrid;
    }

    private void ProjectGrid(SceneView sceneView) {
        //if grid has not been loaded, try to load now
        if (dispGrid.GetWidth() <= 0 && dispGrid.GetHeight() <= 0)
            PreloadGrid();

        //display grid
        Vector2 cellSize = dispGrid.GetCellSize();
        Vector2 offset = new Vector2(-cellSize.x/2, -cellSize.y/2);
        for (int x = 0; x < dispGrid.GetWidth(); x++) {
            for (int y = 0; y < dispGrid.GetHeight(); y++) {
                Vector2 rectOrigin = dispGrid.GetCellWorldPos(x, y) + offset; //get top-left corner of node
                Rect rect = new Rect(rectOrigin.x, rectOrigin.y, cellSize.x, cellSize.y);

                Color faceColor = dispGrid.GetValueAtCoords(x, y).isWalkable ? Color.clear : Color.white;
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
            SavedPathNode node = dispGrid.GetValueAtWorldPos(pos);

            if ((lastEditedX != node.x) || (lastEditedY != node.y)) {
                node.isWalkable = !node.isWalkable;
                dispGrid.SetValueAtCoords(node.x, node.y, node);

                lastEditedX = node.x;
                lastEditedY = node.y;

                //EditorUtility.SetDirty(fileSO);
                ControlEditPanelState();
            }
        }
        else {
            lastEditedX = 0;
            lastEditedY = 0;
        }
    }
    #endregion
}
