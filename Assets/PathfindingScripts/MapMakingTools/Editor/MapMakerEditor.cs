using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

[CustomEditor(typeof(PathfindingMapMaker))]
public class MapMakerEditor : Editor {
    public VisualTreeAsset UXML;

    public override VisualElement CreateInspectorGUI() {
        PathfindingMapMaker mapMaker = (PathfindingMapMaker)target;

        VisualElement root = new VisualElement();
        UXML.CloneTree(root);

        //save map options
        Button saveButton = root.Q<Button>("SaveMapData");
        saveButton.RegisterCallback<MouseUpEvent>((evt) => mapMaker.SaveCurrMapData());

        //new map option
        Button newSaveFileButton = root.Q<Button>("NewMapData");
        newSaveFileButton.RegisterCallback<MouseUpEvent>((evt) => mapMaker.MakeNewSaveFile(false));

        return root;
    }
}
