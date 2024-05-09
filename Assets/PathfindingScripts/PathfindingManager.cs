using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathfindingManager : MonoBehaviour {
    private static PathfindingManager _instance;
    public static PathfindingManager Instance {
        get {
            if (_instance == null) {
                GameObject go = new GameObject("Pathfinding Manager");
                _instance = go.AddComponent<PathfindingManager>();
            }
            return _instance;
        }
    }

    public enum MapLoadingBehaviour { LoadPresavedMapData, CreateMapOnStart }

    [SerializeField] private MapAssignment[] mapAssignments;
    [System.Serializable] private struct MapAssignment {
        public SOMapData mapData;
        public List<PathFollower> pathFollowers;
    }

    private List<Pathfinder> pathfinders = new List<Pathfinder>(); 
    public List<Pathfinder> AllPathfinders { get { return pathfinders; } }

    private void Awake() {
        if (_instance != null) {
            Debug.LogWarning($"An earlier instance of Pathfinding Manager on {_instance.gameObject.name} was destroyed and replaced by one on {gameObject.name}");
            DestroyImmediate(_instance);
        }
        _instance = this;

        //init saved maps in scene
        foreach (MapAssignment assignment in mapAssignments) {
            Pathfinder newPathfinder = new Pathfinder(assignment.mapData.grid);
            pathfinders.Add(newPathfinder);
            foreach (PathFollower follower in assignment.pathFollowers)
                follower.Init(newPathfinder);
        }
       
    }
}
