using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] PathfindingMapMaker map;
    [SerializeField] float walkingSpeed;

    private Rigidbody2D rb;
    private bool isWalking = false;
    private bool mapEditingLastFrame = false;

    private List<PathNode> currPath = new List<PathNode>();
    private int targetIndex;


    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        //set player at map origin
        TeleportPlayerToCell(0, 0);
    }

    private void Update() {

        //if map was just switched to edit mode, move player & stop walking
        if (map.EditMode != mapEditingLastFrame && mapEditingLastFrame == false) {
            CancelWalking();
            TeleportPlayerToCell(0, 0);
            return;
        }
        mapEditingLastFrame = map.EditMode;

        //if map is in edit mode, do not allow player to walk
        if (map.EditMode)
            return;

        if (isWalking) {

            //if player is at end of path, finish walking
            if (targetIndex == currPath.Count) {
                CancelWalking();
                return;
            }

            //if player should reach next node this frame, just place them there and wait for next frame
            if (Vector2.Distance(rb.position, currPath[targetIndex].worldPos) < walkingSpeed * Time.deltaTime) {
                rb.MovePosition(currPath[targetIndex].worldPos);
                targetIndex++;
                return;
            }

            //else, keep walking towards next node
            Vector2 targetPos = currPath[targetIndex].worldPos;
            Vector2 towardsNextNode = (targetPos - rb.position).normalized;
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
        Vector2 worldPos = map.MapGrid.GetCellWorldPos(x, y);
        rb.MovePosition(worldPos);
    }

    private void CancelWalking() {
        isWalking = false;
        currPath.Clear();
        targetIndex = 0;
    }
}
