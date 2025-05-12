using UnityEngine;

public class Cell
{
	public int x;
	public int y;
	public bool walkable;
	public int penalty = 0;
	public GameObject cellObject;
	public bool visited = false;

	public Cell(int x, int y, GameObject cellObject, bool walkable)
	{
		this.x = x;
		this.y = y;
		this.cellObject = cellObject;
		this.walkable = walkable;
	}
}
