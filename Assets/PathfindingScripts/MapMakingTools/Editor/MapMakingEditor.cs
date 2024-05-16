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
    public VisualTreeAsset uxml;

    //layout refs
    EnumField fileTypeField;

    //file handling
    [SerializeField] private SaveUtils.SupportedFileTypes fileType;
    [SerializeField] private SOMapData SOfile;
    [SerializeField] private TextAsset JSONfile;
    const string EXPORT_FOLDER_PATH = "Assets/MapData";

    //map dimensions / visibility
    Grid<SavedPathNode> dispGrid;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] Vector2 cellSize;
    [SerializeField] Vector2 origin;



    #region Layout Updates & Setup
    [MenuItem("Pathfinder Tools/Map Maker")] public static void ShowEditor() {
        EditorWindow wnd = GetWindow<MapMakingEditor>();
        wnd.titleContent = new GUIContent("Map Maker");
    }

    private void CreateGUI() {
        uxml.CloneTree(rootVisualElement);

        //header setup
        fileTypeField = rootVisualElement.Q<EnumField>("filetype");
        fileTypeField.RegisterValueChangedCallback(OnHeaderChanged);
        UpdateHeader();

        //add binding fields from UI Builder
        SerializedObject so = new SerializedObject(this);
        rootVisualElement.Bind(so);
    }

    private void OnHeaderChanged(ChangeEvent<System.Enum> evt) { UpdateHeader((SaveUtils.SupportedFileTypes)evt.newValue); }
    private void UpdateHeader() {
        System.Enum val = fileTypeField.value;
        UpdateHeader((SaveUtils.SupportedFileTypes)val);
    }
    private void UpdateHeader(SaveUtils.SupportedFileTypes overrideValue) {
        fileType = overrideValue;
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                rootVisualElement.Q<ObjectField>("sofile").style.display = DisplayStyle.Flex;
                rootVisualElement.Q<ObjectField>("jsonfile").style.display = DisplayStyle.None;
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                rootVisualElement.Q<ObjectField>("sofile").style.display = DisplayStyle.None;
                rootVisualElement.Q<ObjectField>("jsonfile").style.display = DisplayStyle.Flex;
                break;
        }
    }

    #endregion


    #region File Handling
    
    #endregion
}
