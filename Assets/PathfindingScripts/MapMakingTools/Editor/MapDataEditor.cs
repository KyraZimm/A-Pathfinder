using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
namespace Pathfinding {
    [CustomEditor(typeof(SOMapData))]
    public class MapDataEditor : Editor {
        public VisualTreeAsset UXML;

        public override VisualElement CreateInspectorGUI() {
            SOMapData data = (SOMapData)target;

            VisualElement root = new VisualElement();
            UXML.CloneTree(root);

            IntegerField width = root.Q<IntegerField>("width");
            IntegerField height = root.Q<IntegerField>("height");
            Vector2Field cellsize = root.Q<Vector2Field>("cellsize");
            Vector2Field origin = root.Q<Vector2Field>("origin");

            width.value = data.grid.GetWidth();
            height.value = data.grid.GetHeight();
            cellsize.value = data.grid.GetCellSize();
            origin.value = data.grid.GetOrigin();

            return root;
        }
    }
}
