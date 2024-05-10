using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Pathfinding {

    public static class SaveUtils {
        private const string EXPORT_FOLDER_PATH = "Assets/MapData/";
        public enum SupportedFileTypes { JSON, SO }

        public static void SaveToJson(string jsonName, Pathfinder pathfinder) {
            string path = EXPORT_FOLDER_PATH + jsonName + ".json";
            string contents = JsonUtility.ToJson(pathfinder.ToSaveDataGrid(), true);

            if (!File.Exists(path))
                File.Create(path);
            File.WriteAllTextAsync(path, contents);
        }

        public static void SaveToJson(string jsonName, Grid<SavedPathNode> savedGrid) {
            string path = EXPORT_FOLDER_PATH + jsonName + ".json";
            string contents = JsonUtility.ToJson(savedGrid, true);

            if (!File.Exists(path))
                File.Create(path);
            File.WriteAllTextAsync(path, contents);
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
