using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using static UnityEditor.Timeline.TimelinePlaybackControls;

namespace Pathfinding {

    public static class SaveUtils {
        private const string EXPORT_FOLDER_PATH = "Assets/MapData/";
        public enum SupportedFileTypes { SO, JSON }

        public static string GetFileSuffix(SupportedFileTypes fileType) {
            switch (fileType) {
                case SupportedFileTypes.SO:
                    return ".asset";
                case SupportedFileTypes.JSON:
                    return ".json";
            }

            return "";
        }

        public static void SaveToJson(string jsonName, Pathfinder pathfinder) {
            SaveToJson(jsonName, pathfinder.ToSaveDataGrid());
        }

        public static void SaveToJson(string jsonName, Grid<SavedPathNode> savedGrid) {
            string path = EXPORT_FOLDER_PATH + jsonName + ".json";
            string contents = JsonUtility.ToJson(savedGrid, true);

            if (!File.Exists(path))
                File.CreateText(path);
            File.WriteAllTextAsync(path, contents);
        }

        public static void MakeNewJson(string jsonName, Grid<SavedPathNode> savedGrid) {
            string path = EXPORT_FOLDER_PATH + jsonName + ".json";
            string contents = JsonUtility.ToJson(savedGrid, true);

            using (StreamWriter sw = File.CreateText(path)) {
                sw.Write(contents);
                sw.Close();
            }
                
            AssetDatabase.Refresh();
        }

        /*public static TextAsset FetchJSONAsTextAsset(string jsonName) {
            string path = EXPORT_FOLDER_PATH + jsonName + ".json";
            return Resources.Load<TextAsset>(path);
        }*/


        public static SOMapData MakeNewSOMapFile(string fileName, Grid<SavedPathNode> savedGrid) {
            SOMapData newFile = ScriptableObject.CreateInstance<SOMapData>();
            newFile.grid = savedGrid;

            string path = EXPORT_FOLDER_PATH + fileName + ".asset";
            AssetDatabase.CreateAsset(newFile, path);

            Debug.Log($"New save file {fileName} was created at {path}.");
            AssetDatabase.Refresh();

            return newFile;
        }

        public static Grid<SavedPathNode> GetGridSaveData(Grid<PathNode> gridToSave) {
            Grid<SavedPathNode> savedGrid = new Grid<SavedPathNode>(gridToSave.GetWidth(), gridToSave.GetHeight(), gridToSave.GetCellSize(), gridToSave.GetOrigin());
            for (int x = 0; x < gridToSave.GetWidth(); x++) {
                for (int y = 0; y < gridToSave.GetWidth(); y++) {
                    SavedPathNode node = gridToSave.GetValueAtCoords(x, y).ToSaveData();
                    savedGrid.SetValueAtCoords(x, y, node);
                }
            }
            return savedGrid;
        }
    }
}
