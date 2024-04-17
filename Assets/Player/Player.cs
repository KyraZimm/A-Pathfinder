using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] PathfindingMapMaker map;
    [SerializeField] float walkingSpeed;

    private Rigidbody2D rb;
    private bool isWalking = false;
    private const float DIST_THRESHOLD = 0.01f;
    private Vector2 nodeDisp;

    private List<PathNode> currPath = new List<PathNode>();
    private int targetIndex;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        nodeDisp = map.MapGrid.GetCellSize() / 2;

        //set player at map origin
        TeleportPlayerToCell(0, 0);
    }

    private void Update() {
        if (isWalking) {

            //check if curr path index should be updated
            if (Vector2.Distance(rb.position, currPath[targetIndex].worldPos + nodeDisp) < DIST_THRESHOLD)
                targetIndex++;

            //if player is at end of path, finish walking
            if (targetIndex == currPath.Count) {
                CancelWalking();
                return;
            }

            //else, keep walking towards next node
            Vector2 target = currPath[targetIndex].worldPos + nodeDisp;
            Vector2 towardsNextNode = (target - rb.position).normalized;
            rb.MovePosition(rb.position + (towardsNextNode * walkingSpeed * Time.deltaTime));
        }

        if (Input.GetMouseButtonDown(0)) {
            CancelWalking();

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currPath = map.GetPath(rb.position, mousePos);
            isWalking = true;
        }
    }

    private void TeleportPlayerToCell(int x, int y) {
        CancelWalking();
        Vector2 worldPos = map.MapGrid.GetCellWorldPos(x, y) + nodeDisp;
        rb.MovePosition(worldPos);
    }

    private void CancelWalking() {
        isWalking = false;
        currPath.Clear();
        targetIndex = 0;
    }
}
