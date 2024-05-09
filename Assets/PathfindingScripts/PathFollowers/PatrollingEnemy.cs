using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PatrollingEnemy : PathFollower {
    [Header("Patrol Path")]
    [SerializeField] List<Vector2> patrolPath;
    [SerializeField] bool loopPath;

    private int nextPathIndx = 0;
    private int dir = 1;

    private void Start() {
        //init enemy at start of patrol path
        if (map.Grid.GetValueAtWorldPos(patrolPath[0]).isWalkable)
            MoveToPos(patrolPath[0]);
        else {
            Debug.LogWarning($"Pathfollower {gameObject.name} tried to start on an unwalkable node. Relocating to nearest walkable position.");
            List<PathNode> closestPath = map.GetPath(transform.position, patrolPath[0]);
            MoveToPos(closestPath[closestPath.Count - 1].worldPos);
        }

        
    }

    protected override void Update() {
        base.Update();

        if (!isWalking && queuedPositions.Count == 0) {
            
        }
    }

    public void AddPosToPath() { patrolPath.Add(gameObject.transform.position); }

    public void QueueNextPointInPath() {
        queuedPositions.Enqueue(patrolPath[nextPathIndx]);

        if (nextPathIndx >= patrolPath.Count - 1) {
            dir = loopPath ? 1 : -1;
            
        }
    }

    

/*#if UNITY_EDITOR
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        for (int i = 0; i < patrolPath.Count-1; i++)
            Gizmos.DrawLine(patrolPath[i], patrolPath[i + 1]);
        if (loopPath)
            Gizmos.DrawLine(patrolPath[patrolPath.Count-1], patrolPath[0]);
    }
#endif*/
}
