using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class Grid<T> {
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] Vector2 origin;
    [SerializeField] Vector2 cellSize;

    [SerializeField] Serializable2DArray<T> cells;

    public Grid(int width, int height, Vector2 cellSize, Vector2 origin) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;

        cells = new Serializable2DArray<T>(new T[width, height]);
    }

    public int GetWidth() { return width; }
    public int GetHeight() { return height; }
    public Vector2 GetCellSize() { return cellSize; }
    public Vector2 GetOrigin() { return origin; }

    public Vector2 GetCellWorldPos(int x, int y) {
        return (new Vector2(x + origin.x, y + origin.y) * cellSize) + (cellSize/2); //returns center of cell
    }

    public void GetCellCoords(Vector2 worldPos, out int x, out int y) {
        GetCellCoords(worldPos, out x, out y, false);
    }
    public void GetCellCoords(Vector2 worldPos, out int x, out int y, bool alwaysClampToGrid) {
        x = Mathf.FloorToInt((worldPos.x - origin.x)/ cellSize.x);
        y = Mathf.FloorToInt((worldPos.y - origin.y)/ cellSize.y);

        if (x < 0 || x >= width || y < 0 || y >= height) {
            if (alwaysClampToGrid) {
                x = Mathf.Clamp(x, 0, width - 1);
                y = Mathf.Clamp(y, 0, height - 1);
            }
            else {
                Debug.LogWarning($"The world position ({worldPos}) passed to GetCellCoords was outside the bounds of the grid. Was this intentional?");
            }
        }
            
    }

    public void SetValueAtCoords(int x, int y, T value) {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;
        cells.Array[x, y] = value;
    }

    public void SetValueAtWorldPos(Vector2 worldPos, T value) {
        GetCellCoords(worldPos, out int x, out int y);
        SetValueAtCoords(x, y, value);
    }

    public T GetValueAtCoords(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return default(T);
        return cells.Array[x, y];
    }

    public T GetValueAtWorldPos(Vector2 worldPos) {
        GetCellCoords(worldPos, out int x, out int y);
        return cells.Array[x, y];
    }

}
