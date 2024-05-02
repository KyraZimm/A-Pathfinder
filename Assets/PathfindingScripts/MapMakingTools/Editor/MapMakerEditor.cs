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


        Button uxmlButton = root.Q<Button>("SaveMapData");
        uxmlButton.RegisterCallback<MouseUpEvent>((evt) => mapMaker.SaveNewMapData());

        return root;
    }
}
