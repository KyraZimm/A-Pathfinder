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
    Button saveButton;
    IntegerField widthField;

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

        ObjectField soFileField = rootVisualElement.Q<ObjectField>("sofile");
        soFileField.RegisterValueChangedCallback(OnFileChanged);

        ObjectField jsonFileField = rootVisualElement.Q<ObjectField>("jsonfile");
        jsonFileField.RegisterValueChangedCallback(OnFileChanged);

        UpdateHeader();

        //save func
        saveButton = rootVisualElement.Q<Button>("save");
        saveButton.clicked += SaveMapContents;

        widthField = rootVisualElement.Q<IntegerField>("width");

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

    private void OnInspectorUpdate() { UpdateWindowState(); }

    private void UpdateWindowState() {
        bool saveFilePresent = (fileType == SaveUtils.SupportedFileTypes.SO && SOfile != null) || (fileType == SaveUtils.SupportedFileTypes.JSON && JSONfile != null);
        bool changesMade = EditorUtility.IsDirty(this);

        saveButton.SetEnabled(saveFilePresent && changesMade);
    }

    #endregion


    #region File Handling
    private void OnFileChanged(ChangeEvent<UnityEngine.Object> evt) {
        //try to get grid values from file
        Grid<SavedPathNode> tryReadGrid = null;
        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                if (SOfile == null) return;
                tryReadGrid = SOfile.grid;
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                if (JSONfile == null) return;
                tryReadGrid = JsonUtility.FromJson<Grid<SavedPathNode>>(JSONfile.text);
                break;
        }

        //if grid is present, change curr values to the read file
        if (tryReadGrid != null) {
            width = tryReadGrid.GetWidth();
            height = tryReadGrid.GetHeight();
            cellSize = tryReadGrid.GetCellSize();
            origin = tryReadGrid.GetOrigin();
        }
    }

    private void SaveMapContents() {
        dispGrid = new Grid<SavedPathNode>(width, height, cellSize, origin); //TEMP

        switch (fileType) {
            case SaveUtils.SupportedFileTypes.SO:
                SOfile.grid = dispGrid;
                EditorUtility.SetDirty(SOfile);
                break;
            case SaveUtils.SupportedFileTypes.JSON:
                SaveUtils.SaveToJson(JSONfile.name, dispGrid);
                break;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    #endregion


    #region Visualization & Editing

    #endregion
}
