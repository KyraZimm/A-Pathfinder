using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T> {
    int width;
    int height;
    Vector2 origin;
    Vector2 cellSize;

    T[,] cells;

    public Grid(int width, int height, Vector2 cellSize, Vector2 origin) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;

        cells = new T[width, height];
    }

    public int GetWidth() { return width; }
    public int GetHeight() { return height; }
    public Vector2 GetCellSize() { return cellSize; }

    public Vector2 GetCellWorldPos(int x, int y) {
        return new Vector2(x + origin.x, y + origin.y) * cellSize;
    }

    public void GetCellCoords(Vector2 worldPos, out int x, out int y) {
        x = Mathf.FloorToInt((worldPos.x - origin.x)/ cellSize.x);
        y = Mathf.FloorToInt((worldPos.y - origin.y)/ cellSize.y);
    }

    public void SetValueAtCoords(int x, int y, T value) {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;
        cells[x, y] = value;
    }

    public void SetValueAtWorldPos(Vector2 worldPos, T value) {
        GetCellCoords(worldPos, out int x, out int y);
        SetValueAtCoords(x, y, value);
    }

    public T GetValueAtCoords(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return default(T);
        return cells[x, y];
    }

    public T GetValueAtWorldPos(Vector2 worldPos) {
        GetCellCoords(worldPos, out int x, out int y);
        return cells[x, y];
    }

}
