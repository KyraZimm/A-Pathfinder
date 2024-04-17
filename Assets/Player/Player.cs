using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] PathfindingMapMaker map;
    [SerializeField] float walkingSpeed;

    private Rigidbody2D rb;
    private bool isWalking = false;
    private const float DIST_THRESHOLD = 0.01f;

    private List<PathNode> currPath = new List<PathNode>();
    private int currPathIndex;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        //set player at map origin
        TeleportPlayerToCell(0, 0);
    }

    private void Update() {
        if (isWalking) {
            //check if curr path index should be updated
            if (Vector2.Distance(rb.position, currPath[currPathIndex+1].worldPos) < DIST_THRESHOLD)
                currPathIndex++;

            //if player is at end of path, finish walking
            if (currPathIndex == currPath.Count - 1) {
                CancelWalking();
                return;
            }

            //else, keep walking towards next node
            Vector2 target = currPath[currPathIndex + 1].worldPos;
            Vector2 towardsNextNode = (currPath[currPathIndex+1].worldPos - rb.position).normalized;
            Vector2 moveTo = rb.position + (towardsNextNode * walkingSpeed * Time.deltaTime);
            rb.MovePosition(moveTo);
        }

        if (Input.GetMouseButtonDown(0)) {
            CancelWalking();

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currPath = map.GetPath(rb.position, mousePos);
            isWalking = true;

            Debug.Log($"{currPath.Count} nodes in path");
            for (int i = 1; i < currPath.Count; i++) {
                Debug.DrawLine(currPath[i - 1].worldPos, currPath[i].worldPos, Color.red, 10f);
            }
        }
    }

    private void TeleportPlayerToCell(int x, int y) {
        CancelWalking();
        Vector2 worldPos = map.MapGrid.GetCellWorldPos(x, y);
        rb.MovePosition(worldPos);
    }

    private void CancelWalking() {
        isWalking = false;
        currPath.Clear();
        currPathIndex = 0;
    }
}
