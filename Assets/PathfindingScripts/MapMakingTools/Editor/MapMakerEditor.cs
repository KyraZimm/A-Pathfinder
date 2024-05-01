using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

[CustomEditor(typeof(PathfindingMapMaker))]
public class MapMakerEditor : Editor {
    public VisualTreeAsset UXML;

    public override VisualElement CreateInspectorGUI() {
        VisualElement root = new VisualElement();
        UXML.CloneTree(root);

        return root;
    }
}
