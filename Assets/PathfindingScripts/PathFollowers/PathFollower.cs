using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour {
    [SerializeField] protected float speed;
    [SerializeField] protected MoveMode componentToMove;
    [SerializeField] protected InputMode typeOfInput;

    protected bool canWalk = true;
    protected bool isWalking = false;
    protected Vector2 startPos;

    protected Pathfinder map;
    protected List<PathNode> currPath = new List<PathNode>();
    protected int targetIndex;

    protected enum MoveMode { Transform, RigidbodyPosition, RigidbodyVelocity }
    protected enum InputMode { MouseClick, ManualInput }
    protected Rigidbody2D rb;

    protected Queue<Vector2> queuedPositions = new Queue<Vector2>();
    
    //NOTE: currently, this must be called. Eventually refactor this so it can be compiled on Awake depending on type of PathFollower
    public void Init(Pathfinder connectedMap) {
        map = connectedMap;
        rb = GetComponent<Rigidbody2D>();

        //clamp starting pos to grid, save for later
        map.Grid.GetCellCoords(transform.position, out int x, out int y, true);
        startPos = map.Grid.GetCellWorldPos(x, y);
        MoveToPos(startPos);

        //OnFindNewPath += GetNewPath;
    }

    public void AllowMovement(bool canMove){ canWalk = canMove; }
    public void QueueFutureDestination(Vector2 destToQueue) { queuedPositions.Enqueue(destToQueue); }


    protected virtual void Update() {

        //if object is walking, but no longer allowed, stop walking
        if (!canWalk && isWalking) {
            MoveToPos(startPos, true);
            return;
        }

        //if object should not be walking, skip update
        if (!canWalk)
            return;

        //if object has a queued destination, start walking there
        if (!isWalking && queuedPositions.Count > 0)
            GetNewPath(queuedPositions.Dequeue());
        

        if (isWalking && currPath != null) {

            //if object is at end of path, finish walking
            if (targetIndex == currPath.Count) {
                CancelWalking();
                return;
            }

            //if object should reach next node this frame, update target and wait for next frame
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
            //OnFindNewPath?.Invoke(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            GetNewPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    protected void GetNewPath(Vector2 targetPos) {
        CancelWalking();

        currPath = map.GetPath(transform.position, targetPos);
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
    protected void MoveToPos(Vector2 pos) {
        MoveToPos(pos, false);
    }
    protected void MoveToPos(Vector2 pos, bool cancelWalking) {
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

    private void ClearPositionQueue(){ queuedPositions.Clear(); }
}