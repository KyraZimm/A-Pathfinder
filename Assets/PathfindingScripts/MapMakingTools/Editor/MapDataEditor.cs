using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

[CustomEditor(typeof(PathfindingMapData))]
public class MapDataEditor : Editor {
    public VisualTreeAsset UXML;

    public override VisualElement CreateInspectorGUI() {
        PathfindingMapData data = (PathfindingMapData)target;

        VisualElement root = new VisualElement();
        UXML.CloneTree(root);

        IntegerField width = root.Q<IntegerField>("width");
        IntegerField height = root.Q<IntegerField>("height");
        Vector2Field cellsize = root.Q<Vector2Field>("cellsize");
        Vector2Field origin = root.Q<Vector2Field>("origin");

        Grid<SavedPathNode> savedGrid = data.ToGrid();
        width.value = savedGrid.GetWidth();
        height.value = savedGrid.GetHeight();
        cellsize.value = savedGrid.GetCellSize();
        origin.value = savedGrid.GetOrigin();

        return root;
    }
}
