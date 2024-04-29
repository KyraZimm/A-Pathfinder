using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour {
    [SerializeField] MoveMode componentToMove;
    [SerializeField] float speed;

    private bool canWalk = true;
    private bool isWalking = false;

    private List<PathNode> currPath = new List<PathNode>();
    private int targetIndex;

    Pathfinder map;

    private enum MoveMode { Transform, RigidbodyPosition, RigidbodyVelocity }
    //private enum InputMode { FollowTransform, FollowRigidbody, ManualInput }
    private Rigidbody2D rb;

    public void Init(Pathfinder connectedMap) {
        map = connectedMap;
    }
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {

        //if object is walking, but no longer allowed, stop walking
        if (!canWalk && isWalking) {
            CancelWalking();
            TeleportPlayerToCell(0, 0);
            return;
        }

        if (isWalking) {

            //if player is at end of path, finish walking
            if (targetIndex == currPath.Count) {
                CancelWalking();
                return;
            }

            //if player should reach next node this frame, just place them there and wait for next frame
            if (Vector2.Distance(transform.position, currPath[targetIndex].worldPos) < speed * Time.deltaTime) {
                switch (componentToMove) {
                    case MoveMode.Transform:
                        transform.position = currPath[targetIndex].worldPos;
                        break;
                    case MoveMode.RigidbodyPosition:
                        rb.MovePosition(currPath[targetIndex].worldPos);
                        break;
                    case MoveMode.RigidbodyVelocity:
                        break;
                }

                targetIndex++;
                return;
            }

            //else, keep walking towards next node
            Vector2 targetPos = currPath[targetIndex].worldPos;
            Vector2 towardsNextNode = (targetPos - rb.position).normalized;
            switch (componentToMove) {
                case MoveMode.Transform:
                    transform.position = (Vector2)transform.position + (towardsNextNode * speed * Time.deltaTime);
                    break;
                case MoveMode.RigidbodyPosition:
                    rb.MovePosition(rb.position + (towardsNextNode * speed * Time.deltaTime));
                    break;
                case MoveMode.RigidbodyVelocity:
                    rb.velocity = towardsNextNode * speed;
                    break;
            }
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
        Vector2 worldPos = map.Grid.GetCellWorldPos(x, y);
        rb.MovePosition(worldPos);
    }

    private void CancelWalking() {
        isWalking = false;
        currPath.Clear();
        targetIndex = 0;

        if (componentToMove == MoveMode.RigidbodyVelocity)
            rb.velocity = Vector2.zero;
    }
}