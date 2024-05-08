using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif


public class MapMakingEditor : EditorWindow {
    public VisualTreeAsset testUXML;

    private PathfindingMapData saveFile;
    private Button saveButton;
    private Button newSaveFileButton;

    [MenuItem("Tools/Map Maker")] public static void ShowEditor() {
        EditorWindow wnd = GetWindow<MapMakingEditor>();
        wnd.titleContent = new GUIContent("Map Maker");
    }

    public void CreateGUI() {
        testUXML.CloneTree(rootVisualElement);

        saveButton = rootVisualElement.Q<Button>("savemap");
        //saveButton.RegisterCallback<MouseUpEvent>((evt) => mapMaker.SaveCurrMapData());
        
        newSaveFileButton = rootVisualElement.Q<Button>("createnewmap");
        //newSaveFileButton.RegisterCallback<MouseUpEvent>((evt) => mapMaker.MakeNewSaveFile(forceNewFile: false));

        ObjectField saveFileField = rootVisualElement.Q<ObjectField>("savefile");
        saveFileField.RegisterValueChangedCallback(OnSaveFileChangedEvent);

        PathfindingMapData file = (PathfindingMapData)saveFileField.value;
        SetNewSaveFile(file);
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
