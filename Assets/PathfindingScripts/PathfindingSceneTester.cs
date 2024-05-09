using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PathfindingSceneTester : MonoBehaviour
{
    [SerializeField] GameObject gridCellPrefab;
    [SerializeField] int pathfindingIndexToDraw;

    void Start() {
        Pathfinder pf = PathfindingManager.Instance.AllPathfinders[pathfindingIndexToDraw];

        //draw grid
        for (int x = 0; x < pf.Grid.GetWidth(); x++) {
            for (int y = 0; y < pf.Grid.GetHeight(); y++) {
                GameObject cell = Instantiate(gridCellPrefab, pf.Grid.GetCellWorldPos(x, y), Quaternion.identity);
                cell.transform.localScale = pf.Grid.GetCellSize();
                cell.transform.GetChild(0).gameObject.SetActive(pf.Grid.GetValueAtCoords(x, y).isWalkable);
            }
        }
    }

}
