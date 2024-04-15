using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T>  {
    int width;
    int height;
    Vector2 origin;
    
    float cellSize;

    T[,] nodes;

    public Grid(int width, int height, float cellSize, Vector2 origin) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;

        nodes = new T[width, height];
    }


    public Vector2 GetNodeWorldPos(int x, int y) { //returns world space of grid node
        return new Vector2(x + origin.x, y + origin.y) * cellSize;
    }

    public void GetNodeCoords(Vector2 worldPos, out int x, out int y) { //returns x and y coords of node
        x = Mathf.FloorToInt((worldPos.x - origin.x)/ cellSize);
        y = Mathf.FloorToInt((worldPos.y - origin.y)/ cellSize);
    }

    public void SetValueFromCoords(int x, int y, T value) {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;
        nodes[x, y] = value;
    }

    public void SetValueFromWorldPos(Vector2 worldPos, T value) {
        GetNodeCoords(worldPos, out int x, out int y);
        SetValueFromCoords(x, y, value);
    }

    public T GetValueAtCoords(int x, int y) {
        return nodes[x, y];
    }

    public T GetValueAtWorldPos(Vector2 worldPos) {
        GetNodeCoords(worldPos, out int x, out int y);
        return nodes[x, y];
    }

}
