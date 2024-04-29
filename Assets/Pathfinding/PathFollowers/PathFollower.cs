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
        rb = GetComponent<Rigidbody2D>();
    }
    private void Awake() {
        if (rb == null)
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
                MoveInDirection(currPath[targetIndex].worldPos);
                targetIndex++;
                return;
            }

            //else, keep walking towards next node
            Vector2 targetPos = currPath[targetIndex].worldPos;
            Vector2 towardsNextNode = targetPos - (Vector2)transform.position;
            MoveInDirection(towardsNextNode);
        }

        if (Input.GetMouseButtonDown(0)) {
            CancelWalking();

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currPath = map.GetPath(rb.position, mousePos);
            isWalking = true;
        }
    }

    //use for smoothed movement
    private void MoveInDirection(Vector2 dir) {
        //custom means of normalizing vector, for speed
        double v = Math.Sqrt((dir.x * dir.x) + (dir.y * dir.y));
        if (v > 9.99999974737875E-06) {
            float fv = (float)v;
            dir.x /= fv;
            dir.y /= fv;
        }
        else
            dir = Vector3.zero;

        //move in direction based on physics type
        switch (componentToMove) {
            case MoveMode.Transform:
                transform.position = Vector2.Lerp(transform.position, (Vector2)transform.position + (dir * speed), Time.deltaTime);
                break;
            case MoveMode.RigidbodyPosition:
                rb.MovePosition(rb.position + (dir * speed * Time.deltaTime));
                break;
            case MoveMode.RigidbodyVelocity:
                rb.velocity = dir * speed;
                break;
        }
    }

    //use for immediate placement at pos - unsmoothed
    private void MoveToPos(Vector2 pos) {
        switch (componentToMove) {
            case MoveMode.Transform:
                transform.position = pos;
                break;
            case MoveMode.RigidbodyPosition:
                rb.MovePosition(pos);
                break;
            case MoveMode.RigidbodyVelocity:
                rb.position = pos;
                rb.velocity = Vector2.zero;
                break;
        }
    }

    private void TeleportPlayerToCell(int x, int y) {
        CancelWalking();
        Vector2 worldPos = map.Grid.GetCellWorldPos(x, y);
        MoveToPos(worldPos);
    }

    private void CancelWalking() {
        isWalking = false;
        currPath.Clear();
        targetIndex = 0;

        if (componentToMove == MoveMode.RigidbodyVelocity)
            rb.velocity = Vector2.zero;
    }
}