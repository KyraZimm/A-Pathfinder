using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

public class MapMakingEditor : EditorWindow {
    public VisualTreeAsset testUXML;

    private PathfindingMapData saveFile;

    //layout
    private IMGUIContainer editMapPanel;
    private IMGUIContainer newMapPanel;
    private ToolbarButton editMap;
    private ToolbarButton newMap;

    //ui controls
    private Button saveButton;
    private Button newSaveFileButton;

    [MenuItem("Tools/Map Maker")] public static void ShowEditor() {
        EditorWindow wnd = GetWindow<MapMakingEditor>();
        wnd.titleContent = new GUIContent("Map Maker");
    }

    public void CreateGUI() {
        testUXML.CloneTree(rootVisualElement);

        //set up layout controls
        editMapPanel = rootVisualElement.Q<IMGUIContainer>("editmappanel");
        newMapPanel = rootVisualElement.Q<IMGUIContainer>("newmappanel");
        editMap = rootVisualElement.Q<ToolbarButton>("edit");
        newMap = rootVisualElement.Q<ToolbarButton>("newmap");

        editMap.clicked += DisplayEditPanel;
        newMap.clicked += DisplayNewMapPanel;

        saveButton = rootVisualElement.Q<Button>("savemap");
        //saveButton.RegisterCallback<MouseUpEvent>((evt) => mapMaker.SaveCurrMapData());
        
        newSaveFileButton = rootVisualElement.Q<Button>("createnewmap");
        //newSaveFileButton.RegisterCallback<MouseUpEvent>((evt) => mapMaker.MakeNewSaveFile(forceNewFile: false));

        ObjectField saveFileField = rootVisualElement.Q<ObjectField>("savefile");
        saveFileField.RegisterValueChangedCallback(OnSaveFileChangedEvent);

        PathfindingMapData file = (PathfindingMapData)saveFileField.value;
        SetNewSaveFile(file);

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

    private void OnSaveFileChangedEvent(ChangeEvent<UnityEngine.Object> evt) {
        PathfindingMapData newFile = (PathfindingMapData)evt.newValue;
        SetNewSaveFile(newFile);
    }

    private void SetNewSaveFile(PathfindingMapData newFile) {
        saveFile = newFile;
        saveButton.SetEnabled(saveFile != null);
    }


}
