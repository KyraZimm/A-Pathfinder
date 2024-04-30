using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour {
    [SerializeField] float speed;
    [SerializeField] MoveMode componentToMove;
    [SerializeField] InputMode typeOfInput;

    private bool canWalk = true;
    private bool isWalking = false;
    private Vector2 startPos;

    Pathfinder map;
    private List<PathNode> currPath = new List<PathNode>();
    private int targetIndex;

    private enum MoveMode { Transform, RigidbodyPosition, RigidbodyVelocity }
    private enum InputMode { MouseClick }
    private Rigidbody2D rb;

    System.Action OnFindNewPath;
    
    //NOTE: currently, this must be called. Eventually refactor this so it can be compiled on Awake depending on type of PathFollower
    public void Init(Pathfinder connectedMap) {
        map = connectedMap;
        rb = GetComponent<Rigidbody2D>();

        //clamp starting pos to grid, save for later
        map.Grid.GetCellCoords(transform.position, out int x, out int y, true);
        startPos = map.Grid.GetCellWorldPos(x, y);

        OnFindNewPath += GetNewPath;
    }

    public void AllowMovement(bool canMove){ canWalk = canMove; }


    private void Update() {

        //if object is walking, but no longer allowed, stop walking
        if (!canWalk && isWalking) {
            MoveToPos(startPos, true);
            return;
        }

        if (isWalking && currPath != null) {

            //if player is at end of path, finish walking
            if (targetIndex == currPath.Count) {
                CancelWalking();
                return;
            }

            //if player should reach next node this frame, update target and wait for next frame
            if (Vector2.Distance(transform.position, currPath[targetIndex].worldPos) < speed * Time.deltaTime) {
                targetIndex++;
                return;
            }

            //else, keep walking towards next node
            Vector2 targetPos = currPath[targetIndex].worldPos;
            Vector2 towardsNextNode = targetPos - (Vector2)transform.position;
            MoveInDirection(towardsNextNode);
        }

        //TEMP. As more input types are added, this will be refactored to accomodate new types
        //Will also have to refactor to accomodate Input Manager vs. Input System
        if (typeOfInput == InputMode.MouseClick && Input.GetMouseButtonDown(0)) {
            OnFindNewPath?.Invoke();
        }
    }

    private void GetNewPath() {
        CancelWalking();

        switch (typeOfInput) {
            case InputMode.MouseClick:
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                currPath = map.GetPath(rb.position, mousePos);
                break;
        }

        isWalking = true;
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
        MoveToPos(pos, false);
    }
    private void MoveToPos(Vector2 pos, bool cancelWalking) {
        if (cancelWalking)
            CancelWalking();

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

    private void CancelWalking() {
        isWalking = false;
        targetIndex = 0;

        if (currPath != null)
            currPath.Clear();

        //shouldn't be necessary, but just in case:
        if (componentToMove == MoveMode.RigidbodyVelocity)
            rb.velocity = Vector2.zero;
    }
}