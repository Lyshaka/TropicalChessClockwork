using System;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
	const int STRAIGHT_COST = 100;
	const int DIAGONAL_COST = 150;

	public Cell[,] grid;
	
	GameGrid _gameGrid;

	private void Awake()
	{
		_gameGrid = GetComponent<GameGrid>();
		grid = _gameGrid.gridCell;
	}

	public void FindPath(PathRequest request, Action<PathResult> callback)
	{
		Vector2Int[] waypoints = new Vector2Int[0];
		bool pathSuccess = false;

		Node[,] nodes = new Node[GameGrid.GRID_SIZE, GameGrid.GRID_SIZE];

		for (int x = 0; x < GameGrid.GRID_SIZE; x++)
			for (int y = 0; y < GameGrid.GRID_SIZE; y++)
				nodes[x, y] = new(grid[x, y].walkable, new(x, y), grid[x, y].penalty);

		Node startNode = nodes[request.pathStart.x, request.pathStart.y];
		Node targetNode = nodes[request.pathTarget.x, request.pathTarget.y];

		if (startNode.walkable && targetNode.walkable)
		{
			Heap<Node> openSet = new(GameGrid.GRID_SIZE * GameGrid.GRID_SIZE);
			HashSet<Node> closedSet = new();

			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				if (currentNode == targetNode) // Found Path !
				{
					pathSuccess = true;
					break;
				}

				foreach (Node neighbor in GetNeighbors(currentNode))
				{
					if (!neighbor.walkable || closedSet.Contains(neighbor))
						continue;

					int costToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.penalty;
					if (costToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
					{
						neighbor.gCost = costToNeighbor;
						neighbor.hCost = GetDistance(neighbor, targetNode);
						neighbor.parent = currentNode;

						if (!openSet.Contains(neighbor))
							openSet.Add(neighbor);
						else
							openSet.UpdateItem(neighbor);
					}
				}
			}
		}

		if (pathSuccess)
		{
			waypoints = RetracePath(startNode, targetNode);
			pathSuccess = waypoints.Length > 0;
		}

		callback(new PathResult(waypoints, pathSuccess, request.callback));
		return;

		// Utilities
		List<Node> GetNeighbors(Node node)
		{
			List<Node> neighbors = new();

			int x = node.gridPos.x;
			int y = node.gridPos.y;

			bool left = x > 0 && grid[x - 1, y].walkable;
			bool right = x < GameGrid.GRID_SIZE - 1 && grid[x + 1, y].walkable;
			bool up = y < GameGrid.GRID_SIZE - 1 && grid[x, y + 1].walkable;
			bool down = y > 0 && grid[x, y - 1].walkable;

			if (left) neighbors.Add(nodes[x - 1, y]);
			if (right) neighbors.Add(nodes[x + 1, y]);
			if (up) neighbors.Add(nodes[x, y + 1]);
			if (down) neighbors.Add(nodes[x, y - 1]);

			// Check diagonals only if both adjacent cardinal directions are walkable
			if (left && up) neighbors.Add(nodes[x - 1, y + 1]);
			if (right && up) neighbors.Add(nodes[x + 1, y + 1]);
			if (left && down) neighbors.Add(nodes[x - 1, y - 1]);
			if (right && down) neighbors.Add(nodes[x + 1, y - 1]);

			return neighbors;
		}

		int GetDistance(Node a, Node b)
		{
			int dstX = Mathf.Abs(a.gridPos.x - b.gridPos.x);
			int dstY = Mathf.Abs(a.gridPos.y - b.gridPos.y);

			if (dstX > dstY)
				return (DIAGONAL_COST * dstY + STRAIGHT_COST * (dstX - dstY));
			return (DIAGONAL_COST * dstX + STRAIGHT_COST * (dstY - dstX));
		}
	}

	Vector2Int[] RetracePath(Node startNode, Node endNode)
	{
		List<Vector2Int> waypoints = new();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			waypoints.Add(new(currentNode.gridPos.x, currentNode.gridPos.y));
			currentNode = currentNode.parent;
		}

		waypoints.Reverse();

		return waypoints.ToArray();
	}
}