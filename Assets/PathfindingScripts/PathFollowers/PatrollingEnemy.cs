using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PatrollingEnemy : PathFollower {
    [Header("Patrol Path")]
    [SerializeField] List<Vector2> patrolPath;
    [SerializeField] bool loopPath;

    private int pathIndx = 0;
    private int dir = 1;

    private void Start() {

        //init with enemy patrolling path
        if (map.Grid.GetValueAtWorldPos(patrolPath[0]).isWalkable)
            MoveToPos(patrolPath[0]);
        else {
            Debug.LogWarning($"Pathfollower {gameObject.name} tried to start on an unwalkable node.");
        }
    }

    protected override void Update() {
        base.Update();

        //if player is 
        /*if () {
            
        }*/
    }

    public void AddPosToPath() { patrolPath.Add(gameObject.transform.position); }

#if UNITY_EDITOR
    private void OnGUI() {
        for (int i = 0; i < patrolPath.Count-1; i++)
            Debug.DrawLine(patrolPath[i], patrolPath[i + 1], Color.red);
        if (loopPath)
            Debug.DrawLine(patrolPath[patrolPath.Count-1], patrolPath[0], Color.red);
    }
#endif
}
