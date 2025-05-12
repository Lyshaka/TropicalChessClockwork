using UnityEngine;

public class Node : IHeapItem<Node>
{
	public bool walkable;
	public Vector2Int gridPos;
	public int penalty;
	private int heapIndex;

	public int gCost;
	public int hCost;
	public int fCost => gCost + hCost;
	public Node parent;



	public Node(bool walkable, Vector2Int gridPos, int penalty)
	{
		this.walkable = walkable;
		this.gridPos = gridPos;
		this.penalty = penalty;
	}

	public int HeapIndex
	{
		get { return heapIndex; }
		set { heapIndex = value; }
	}

	public int CompareTo(Node nodeToCompare)
	{
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
			compare = hCost.CompareTo(nodeToCompare.hCost);
		return -compare;
	}
}