using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
    public class Pathfinder {

        const int MOVE_STRAIGHT_COST = 10;
        const int MOVE_DIAGONAL_COST = 14;

        public Grid<PathNode> Grid { get; private set; }
        private List<PathNode> openNodes;
        private List<PathNode> closedNodes;

        #region Constructors & Save Data
        public Pathfinder(int width, int height, Vector2 cellSize, Vector2 origin) {
            Grid = new Grid<PathNode>(width, height, cellSize, origin);

            //fill pathnodes
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    PathNode node = new PathNode(x, y, Grid);
                    Grid.SetValueAtCoords(x, y, node);
                }
            }
        }

        public Pathfinder(Grid<PathNode> grid) {
            Grid = grid;
        }
        public Pathfinder(Grid<SavedPathNode> savedGrid) {
            Grid = new Grid<PathNode>(savedGrid.GetWidth(), savedGrid.GetHeight(), savedGrid.GetCellSize(), savedGrid.GetOrigin());

            for (int x = 0; x < savedGrid.GetWidth(); x++) {
                for (int y = 0; y < savedGrid.GetHeight(); y++) {
                    PathNode node = new PathNode(savedGrid.GetValueAtCoords(x, y), Grid);
                    Grid.SetValueAtCoords(x, y, node);
                }
            }
        }

        public Grid<SavedPathNode> ToSaveDataGrid() {
            Grid<SavedPathNode> gridToSave = new Grid<SavedPathNode>(Grid.GetWidth(), Grid.GetHeight(), Grid.GetCellSize(), Grid.GetOrigin());
            for (int x = 0; x < Grid.GetWidth(); x++) {
                for (int y = 0; y < Grid.GetHeight(); y++) {
                    SavedPathNode nodeToSave = Grid.GetValueAtCoords(x, y).ToSaveData();
                    gridToSave.SetValueAtCoords(x, y, nodeToSave);
                }
            }
            return gridToSave;
        }
        #endregion

        #region Pathfinding Calculations
        private PathNode GetNode(int x, int y) {
            return Grid.GetValueAtCoords(x, y);
        }
        public List<PathNode> GetPath(Vector2 startWorldPos, Vector2 endWorldPos) {
            Grid.GetCellCoords(startWorldPos, out int startX, out int startY);
            Grid.GetCellCoords(endWorldPos, out int endX, out int endY);
            return GetPath(startX, startY, endX, endY);
        }

        public List<PathNode> GetPath(int startX, int startY, int endX, int endY) {
            PathNode startNode = Grid.GetValueAtCoords(startX, startY);
            PathNode endNode = Grid.GetValueAtCoords(endX, endY);

            //if a position from outside the grid was passed in, do not search grid
            if (startNode == null || endNode == null) {
                Debug.LogWarning($"Pathfinder bounds do not cover the space between ({startX}, {startY}) and ({endX}, {endY}). Returning null path.");
                return null;
            }
            
            openNodes = new List<PathNode>() { startNode };
            closedNodes = new List<PathNode>();

            //reset grid values from prior searches
            for (int x = 0; x < Grid.GetWidth(); x++) {
                for (int y = 0; y < Grid.GetHeight(); y++) {
                    PathNode node = Grid.GetValueAtCoords(x, y);
                    node.gCost = int.MaxValue;
                    node.previousNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = DistanceCost(startNode, endNode);

            //if chosen destination is not walkable, find the nearest walkable node
            if (!endNode.isWalkable) {
                List<PathNode> neighbors = GetNeighborNodes(endNode);
                PathNode candidate = GetLowestFCostNode(neighbors);

                while (!candidate.isWalkable) {
                    neighbors = GetNeighborNodes(candidate);
                    candidate = GetLowestFCostNode(neighbors);
                }
                endNode = candidate;
            }

            while (openNodes.Count > 0) {
                PathNode currNode = GetLowestFCostNode(openNodes);
                if (currNode == endNode)
                    return CalculatePath(endNode);

                openNodes.Remove(currNode);
                closedNodes.Add(currNode);

                //search neighboring nodes for A) closest to end node & B) cheapest to walk to
                List<PathNode> neighbors = GetNeighborNodes(currNode);
                foreach (PathNode neighbor in neighbors) {
                    if (closedNodes.Contains(neighbor))
                        continue;
                    if (!neighbor.isWalkable) {
                        closedNodes.Add(neighbor);
                        continue;
                    }

                    int approxGCost = currNode.gCost + DistanceCost(currNode, neighbor);
                    if (approxGCost < neighbor.gCost) {
                        neighbor.previousNode = currNode;
                        neighbor.gCost = approxGCost;
                        neighbor.hCost = DistanceCost(neighbor, endNode);

                        if (!openNodes.Contains(neighbor))
                            openNodes.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private List<PathNode> CalculatePath(PathNode endNode) {
            List<PathNode> path = new List<PathNode>(){ endNode };

            PathNode currNode = endNode;
            while (currNode.previousNode != null) {
                path.Add(currNode.previousNode);
                currNode = currNode.previousNode;
            }

            path.Reverse();
            return path;
        }

        private int DistanceCost(PathNode from, PathNode to) {
            int xDist = Mathf.Abs(from.x - to.x);
            int yDist = Mathf.Abs(from.y - to.y);
            int remaining = Mathf.Abs(xDist - yDist);
            return (MOVE_DIAGONAL_COST * Mathf.Min(xDist, yDist)) + (MOVE_STRAIGHT_COST * remaining);
        }

        private PathNode GetLowestFCostNode(List<PathNode> nodes) {
            PathNode lowest = nodes[0];
            for (int i = 0; i < nodes.Count; i++) {
                if (nodes[i].fCost < lowest.fCost)
                    lowest = nodes[i];
            }

            return lowest;
        }

        private List<PathNode> GetNeighborNodes(PathNode node) {
            List<PathNode> neighbors = new List<PathNode>();

            if (node.x > 0) {
                neighbors.Add(GetNode(node.x - 1, node.y)); //west node
                if (node.y + 1 < Grid.GetHeight())
                    neighbors.Add(GetNode(node.x - 1, node.y + 1)); //northwest node
                if (node.y > 0)
                    neighbors.Add(GetNode(node.x - 1, node.y - 1)); //southwest node
            }

            if (node.x + 1 < Grid.GetWidth()) {
                neighbors.Add(GetNode(node.x + 1, node.y)); //east node
                if (node.y + 1 < Grid.GetHeight())
                    neighbors.Add(GetNode(node.x + 1, node.y + 1)); //northeast node
                if (node.y > 0)
                    neighbors.Add(GetNode(node.x + 1, node.y - 1)); //southeast node
            }

            if (node.y + 1 < Grid.GetHeight())
                neighbors.Add(GetNode(node.x, node.y + 1)); //north node
            if (node.y > 0)
                neighbors.Add(GetNode(node.x, node.y - 1)); //south node

            return neighbors;
        }
        #endregion
    }
}
