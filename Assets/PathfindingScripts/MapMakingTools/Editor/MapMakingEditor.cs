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
    

    [MenuItem("Pathfinder Tools/Map Maker")] public static void ShowEditor() {
        EditorWindow wnd = GetWindow<MapMakingEditor>();
        wnd.titleContent = new GUIContent("Map Maker");
    }

    public void CreateGUI() {
        testUXML.CloneTree(rootVisualElement);

        //set up layout controls
        editMapPanel = rootVisualElement.Q<IMGUIContainer>("editmappanel");
        newMapPanel = rootVisualElement.Q<IMGUIContainer>("newmappanel");

        ToolbarButton editMap = rootVisualElement.Q<ToolbarButton>("edit");
        ToolbarButton newMap = rootVisualElement.Q<ToolbarButton>("newmap");
        editMap.clicked += DisplayEditPanel;
        newMap.clicked += DisplayNewMapPanel;

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


}
