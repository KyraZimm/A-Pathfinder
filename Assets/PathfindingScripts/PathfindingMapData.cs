using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map Data", menuName = "ScriptableObjects/Map Data")]
public class PathfindingMapData : ScriptableObject {
   public Grid grid;
}
